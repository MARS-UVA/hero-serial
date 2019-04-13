/**
 * Example HERO application can reads a serial port and echos the bytes back.
 * After deploying this application, the user can open a serial terminal and type while the HERO echoes the typed keys back.
 * Use a USB to UART (TTL) cable like the Adafruit Raspberry PI or FTDI-TTL cable.
 * Use device manager to figure out which serial port number to select in your PC terminal program.
 * HERO Gadgeteer Port 1 is used in this example, but can be changed at the top of Main().
 */
using System;

namespace HERO_Serial_Example1
{
    public class Program
    {
        // headers: two bytes of 0xff
        static readonly byte H1 = 255;
        static readonly byte H2 = 255;

        // 2 headers + 1 checksum + 4 motor bytes
        static readonly byte PKG_LENGTH = 3 + 4;
        static readonly byte offset = 127;

        static CTRE.Phoenix.MotorControl.CAN.TalonSRX[] myTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX[4];

        /** Serial object, this is constructed on the serial number. */
        static System.IO.Ports.SerialPort _uart;
        /** Ring buffer holding the bytes to transmit. */
        static byte[] _tx = new byte[1024];
        static int _txIn = 0;
        static int _txOut = 0;
        static int _txCnt = 0;
        /** Cache for reading out bytes in serial driver. */
        static byte[] _rx = new byte[1024];
        
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
        /** entry point of the application */
        public static void Main()
        {
            for (int i = 0; i < myTalon.Length; i++)
            {
                myTalon[i] = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(i + 1);
                if (i == 2 || i == 3)
                {
                    myTalon[i].SetInverted(true);
                }
            }

            /* temporary array */
            byte[] scratch = new byte[PKG_LENGTH * 2];

            _uart = new System.IO.Ports.SerialPort(CTRE.HERO.IO.Port1.UART, 115200);
            _uart.Open();

            byte[] packet = new byte[PKG_LENGTH - 2];

            // current packet 
            byte[] curPacket = new byte[PKG_LENGTH - 2];
            // dont actually need to assign the check sum
            for (int i = 0; i < curPacket.Length - 1; i++)
            {
                curPacket[i] = offset;
            }

            while (true)
            {
                /* read bytes out of uart */
                if (_uart.BytesToRead > 0)
                {
                    int readCnt = _uart.Read(_rx, 0, CalcRemainingCap());
                    for (int i = 0; i < readCnt; ++i)
                    {
                        PushByte(_rx[i]);
                    }
                }
                // a complete packet is guaranteed to exist in the buffer if buffer length > 2 * pkg length
                if (_txCnt >= PKG_LENGTH * 2)
                {
                    for (int i = 0; i < scratch.Length; i++)
                    {
                        scratch[i] = PopByte();
                    }
                    bool hasMsg = false;
                    for (int i = 0; i < scratch.Length; i++)
                    {
                        // if headers match, copy the next PKG_LENGTH-2 bytes to packet
                        if (scratch[i] == H1 && scratch[i+1] == H2 && i <= PKG_LENGTH)
                        {
                            for (int j = 0; j < packet.Length; j++)
                            {
                                // add 2 because we dont need the header
                                packet[j] = scratch[i + 2 + j];
                            }
                            hasMsg = true;
                            break;
                        }
                    }
                    if (hasMsg)
                    {
                        if (CheckValidity(packet))
                        {
                            // copy the valid packet to curPacket which is continuously fed to the Talons
                            Array.Copy(packet, curPacket, PKG_LENGTH - 2);
                            // Debug.Print("Valid data:" + packet[0].ToString());
                        }
                    }
                    
                }
                WriteToTalon(curPacket);
                System.Threading.Thread.Sleep(20);
            }
        }
        /**
         * write packets to Talon, ignoring the checksum
         * */
        private static void WriteToTalon(byte[] packet)
        {
            for (int i = 0; i < packet.Length - 1; i++)
            {
                double curByte = (packet[i] - offset);
                if (curByte > 0)
                {
                    myTalon[i].SetInverted(false);
                    myTalon[i].Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, curByte / 100);
                }
                else
                {
                    myTalon[i].SetInverted(true);
                    myTalon[i].Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, -curByte / 100);
                }
            }
            CTRE.Phoenix.Watchdog.Feed();
        }
        // validate the checksum
        private static bool CheckValidity(byte[] packet)
        {
            int lastByte = packet.Length - 1;
            int sum = packet[lastByte];
            int s = 0;
            for (int i = 0; i < lastByte; i++)
            {
                s += packet[i];
            }
            return s % 255 == sum;
        }
    }
}
