/**
 * Example HERO application can reads a serial port and echos the bytes back.
 * After deploying this application, the user can open a serial terminal and type while the HERO echoes the typed keys back.
 * Use a USB to UART (TTL) cable like the Adafruit Raspberry PI or FTDI-TTL cable.
 * Use device manager to figure out which serial port number to select in your PC terminal program.
 * HERO Gadgeteer Port 1 is used in this example, but can be changed at the top of Main().
 */
using System;
using System.Threading;
using Microsoft.SPOT;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;

namespace HERO_Serial
{
    public class Program
    {
        /** Serial object, this is constructed on the serial number. */
        static System.IO.Ports.SerialPort _uart;

        static readonly GameController gamepad = new GameController(new CTRE.Phoenix.UsbHostDevice(0));
        static readonly TalonSRX[] talon = new TalonSRX[7];

        /** Ring buffer holding the bytes to transmit. */
        static byte[] _tx = new byte[1024];
        static int _txIn = 0;
        static int _txOut = 0;
        static int _txCnt = 0;
        /** Cache for reading out bytes in serial driver. */
        static byte[] _rx = new byte[1024];
        /* initial message to send to the terminal */
        static byte[] _helloMsg = MakeByteArrayFromString("HERO_Serial - Start Typing and HERO will echo the letters back.\r\n");
        /** @return the maximum number of bytes we can read*/
        private static int CalcRemainingCap()
        {
            /* firs calc the remaining capacity in the ring buffer */
            int rem = _tx.Length - _txCnt;
            /* cap the return to the maximum capacity of the rx array */
            if (rem > _rx.Length)
                rem = _rx.Length;
            return rem;
        }
        /** @param received byte to push into ring buffer */
        private static void PushByte(byte datum)
        {
            _tx[_txIn] = datum;
            if (++_txIn >= _tx.Length)
                _txIn = 0;
            ++_txCnt;
        }
        /** 
         * Pop the oldest byte out of the ring buffer.
         * Caller must ensure there is at least one byte to pop out by checking _txCnt.
         * @return the oldest byte in buffer.
         */
        private static byte PopByte()
        {
            byte retval = _tx[_txOut];
            if (++_txOut >= _tx.Length)
                _txOut = 0;
            --_txCnt;
            return retval;
        }
        public static void readFromSerial()
        {
            /* temporary array */
            byte[] temp = new byte[1024];
            int tempEnd = 0;
            /* open the UART, select the com port based on the desired gadgeteer port.
             *   This utilizes the CTRE.IO Library.
             *   The full listing of COM ports on HERO can be viewed in CTRE.IO
             *   
             */
            _uart = new System.IO.Ports.SerialPort(CTRE.HERO.IO.Port1.UART, 115200);
            _uart.Open();
            /* send a message to the terminal for the user to see */
            _uart.Write(_helloMsg, 0, _helloMsg.Length);
            /* loop forever */
            while (true)
            {
                /* read bytes out of uart */
                if (_uart.BytesToRead > 0)
                {
                    int readCnt = _uart.Read(_rx, 0, CalcRemainingCap());
                    for (int i = 0; i < readCnt; ++i)
                    {
                        temp[tempEnd++] = _rx[i];
                    }
                }
                /* if there are bufferd bytes echo them back out */
                while (tempEnd > 2)
                {
                    bool flag = false;
                    for (int i = 0; i < tempEnd - 2; i++)
                    {
                        if (temp[i] == 0xff && (temp[i + 1] & 0xC0) == 0xC0)
                        {
                            int count = temp[i + 1] & 0x3F;
                            int expected_len = 2 + count + 1;
                            if (tempEnd >= i + expected_len)
                            {
                                processBytes(temp, i, i + expected_len);
                                for (int j = i; j < i + expected_len; j++)
                                    temp[j - i] = temp[j];
                                tempEnd -= i + expected_len;
                                flag = true;
                                break;
                            }
                            else
                            {
                                // remove bytes already read
                                for (int j = i; j < tempEnd; j++)
                                {
                                    temp[j - i] = temp[j];
                                }
                                tempEnd -= i;
                            }
                        }
                    }
                    if (!flag)
                        tempEnd = 0;
                }

                //if (_uart.CanWrite && (_txCnt > 0))
                //{
                //    scratch[0] = PopByte();
                //    _uart.Write(scratch, 0, 1);
                //}
                /* wait a bit, keep the main loop time constant, this way you can add to this example (motor control for example). */
                CTRE.Phoenix.Watchdog.Feed();
                Thread.Sleep(10);
            }
        }
        public static void processBytes(byte[] data, int start, int end)
        {
            int s = 0;
            for (int i = start; i < end - 1; i++)
            {
                s += data[i];
            }
            string asd = "";
            for (int i = start; i < end; i++)
            {
                asd += data[i] + ",";
            }
            Debug.Print(asd);
            if (s % 256 == data[end - 1])
            {
                Debug.Print("Checksum is correct");
                for (int i = start + 2; i < end - 1; i++)
                {
                    float val = data[i];
                    val = (val - 100) / 100;
                    talon[i - start - 2].Set(ControlMode.PercentOutput, val);
                }
            } else
            {
                Debug.Print("Checksum is incorrect");
            }
        }
        public static float abs(float a)
        {
            return a < 0 ? -a : a;
        }
        public static float thresh(float a, float th)
        {
            return abs(a) < th ? 0 : a;
        }
        public static string ArrToString(float[] arr)
        {
            string val = "";
            for (int i = 0; i < arr.Length; i++)
            {
                val += arr[i] + ", ";
            }
            return val;
        }
        public static void handleGamepad()
        {

            var myGamepad = new GameController(new CTRE.Phoenix.UsbHostDevice(0));
            var temp = new GameControllerValues();
            while (true)
            {
                if (myGamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                {

                    /* print the axis value */
                    temp = myGamepad.GetAllValues(ref temp);
                    float rX = temp.axes[2];
                    float rY = temp.axes[5];
                    talon[0].Set(ControlMode.PercentOutput, thresh(rY - rX, 0.1f));
                    talon[1].Set(ControlMode.PercentOutput, thresh(rY - rX, 0.1f));
                    talon[2].Set(ControlMode.PercentOutput, thresh(rY + rX, 0.1f));
                    talon[3].Set(ControlMode.PercentOutput, thresh(rY + rX, 0.1f));

                    uint buttons = temp.btns;
                    if ((buttons & 8) != 0) // Y
                    {
                        talon[4].Set(ControlMode.PercentOutput, 1.0f);
                    }
                    else if ((buttons & 2) != 0) // A
                    {
                        talon[4].Set(ControlMode.PercentOutput, -1.0f);
                    }
                    else
                    {
                        talon[4].Set(ControlMode.PercentOutput, 0.0f);
                    }
                    if ((buttons & 16) != 0) // LB
                    {
                        talon[5].Set(ControlMode.PercentOutput, 1.0f);
                    }
                    else if ((buttons & 32) != 0) // RB
                    {
                        talon[5].Set(ControlMode.PercentOutput, -1.0f);
                    }
                    else
                    {
                        talon[5].Set(ControlMode.PercentOutput, 0.0f);
                    }
                    talon[6].Set(ControlMode.PercentOutput, thresh(temp.axes[1], 0.1f));

                    Debug.Print("axis:" + ArrToString(temp.axes));
                    Debug.Print("buttons: " + temp.btns);
                    
                }
            }
        }
        /** entry point of the application */
        public static void Main()
        {
            {
                // 16: bucket ladder
                // 17: bucket ladder angle
                int[] talonIdx = { 11, 12, 13, 14, 15, 16, 17 };
                bool[] inverted = { true, true, false, false, false, false, false };
                for (int i = 0; i < 7; i++)
                {
                    var t = new TalonSRX(talonIdx[i]);
                    t.SetInverted(inverted[i]);
                    talon[i] = t;
                }
            }
            readFromSerial();
        }
        /**
         * Helper routine for creating byte arrays from strings.
         * @param msg string message to covnert.
         * @return byte array version of string.
         */
        private static byte[] MakeByteArrayFromString(String msg)
        {
            byte[] retval = new byte[msg.Length];
            for (int i = 0; i < msg.Length; ++i)
                retval[i] = (byte)msg[i];
            return retval;
        }
    }
}
