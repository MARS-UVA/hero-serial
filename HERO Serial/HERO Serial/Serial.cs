using CTRE.Phoenix;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using System.Threading;
using System;

namespace HERO_Serial
{
    class Serial
    {
        /** Serial object, this is constructed on the serial number. */
        static System.IO.Ports.SerialPort _uart;
    
        public static void readFromSerial(TalonSRX[] talons)
        {
            /* temporary array */
            byte[] temp = new byte[2048];
            byte[] dataOut = new byte[talons.Length];
            int tempEnd = 0;
            /* open the UART, select the com port based on the desired gadgeteer port.
             *   This utilizes the CTRE.IO Library.
             *   The full listing of COM ports on HERO can be viewed in CTRE.IO
             */
            _uart = new System.IO.Ports.SerialPort(CTRE.HERO.IO.Port1.UART, 115200);
            _uart.Open();

            /* loop forever */
            while (true)
            {
                /* read bytes out of uart */
                if (_uart.BytesToRead > 0)
                {
                    int capacity = 2048 - tempEnd;
                    // read 64 byte chuck at a time
                    int readCnt = _uart.Read(temp, tempEnd, capacity > 64 ? 64 : 0);
                    tempEnd += readCnt;
                }
                while (tempEnd > 2)
                {
                    int flag = 0;
                    for (int i = 0; i < tempEnd - 2; i++)
                    {
                        if (temp[i] == 0xff && (temp[i + 1] & 0xC0) == 0xC0) // first two significant bits should be set to 1
                        {
                            int count = temp[i + 1] & 0x3F;
                            int expected_len = 2 + count + 1;
                            if (tempEnd >= i + expected_len)
                            {
                                processBytes(talons, temp, i, i + expected_len);
                                // equivalent to temp = temp[i + expected_len:] in python
                                tempEnd -= i + expected_len;
                                Array.Copy(temp, i + expected_len, temp, 0, tempEnd);
                                flag = 1;
                            }
                            else // not enough bytes, need to wait for incoming bytes
                            {
                                // remove bytes already read
                                tempEnd -= i;
                                Array.Copy(temp, i, temp, 0, tempEnd);
                                flag = 2;
                            }
                            break;
                        }
                    }
                    if (flag == 0)
                        tempEnd = 0;
                    if (flag == 2)
                        break;
                }
                Watchdog.Feed();
                if (_uart.CanWrite)
                {
                    for (int i = 0; i < talons.Length; i++)
                        dataOut[i] = (byte)(talons[i].GetOutputCurrent() / 0.25);
                    
                    var encoded = sendBytes(dataOut);
                    _uart.Write(encoded, 0, encoded.Length);
                }
                /* wait a bit, keep the main loop time constant, this way you can add to this example (motor control for example). */
                Thread.Sleep(10);
            }
        }
        public static byte[] sendBytes(byte[] data)
        {
            var encoded = new byte[data.Length + 3];
            encoded[0] = 0xff;
            encoded[1] = (byte)(data.Length | 192);
            Array.Copy(data, 0, encoded, 2, data.Length);
            encoded[encoded.Length - 1] = (byte)(sum(encoded, 0, encoded.Length - 1) % 256);
            return encoded;
        }
        public static int sum(byte[] data, int start, int end)
        {
            int s = 0;
            for (int i = start; i < end; i++)
                s += data[i];
            return s;
        }
        public static void processBytes(TalonSRX[] talons, byte[] data, int start, int end)
        {
            string asd = "";
            for (int i = start; i < end; i++)
                asd += data[i] + ",";
            Debug.Print(asd); // debug purposes

            // excluding the checksum 
            if (sum(data, start, end - 1) % 256 == data[end - 1])
            {
                Debug.Print("Checksum is correct");
                for (int i = start + 2; i < end - 1; i++) // traverse only the data bytes
                {
                    float val = data[i];
                    val = (val - 100) / 100;
                    talons[i - start - 2].Set(ControlMode.PercentOutput, val);
                    
                }
            }
            else
                Debug.Print("Checksum is incorrect");
        }
    }
}
