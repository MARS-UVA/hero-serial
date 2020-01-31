/**
 * Example HERO application can reads a serial port and echos the bytes back.
 * After deploying this application, the user can open a serial terminal and type while the HERO echoes the typed keys back.
 * Use a USB to UART (TTL) cable like the Adafruit Raspberry PI or FTDI-TTL cable.
 * Use device manager to figure out which serial port number to select in your PC terminal program.
 * HERO Gadgeteer Port 1 is used in this example, but can be changed at the top of Main().
 */
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.Sensors;
using Microsoft.SPOT;
using System.Threading;
using System;

namespace HERO_Serial
{
    public class Program
    {
        static readonly TalonSRX[] talons = new TalonSRX[7];
        static readonly PigeonIMU pigeon;

        static Program() {
            // 16: bucket ladder
            // 17: bucket ladder angle
            int[] talonIdx = { 11, 12, 13, 14, 15, 16, 17 };
            bool[] inverted = { true, true, false, false, false, false, false };
            for (int i = 0; i < 7; i++)
            {
                var t = new TalonSRX(talonIdx[i]);
                t.SetInverted(inverted[i]);
                talons[i] = t;
            }
            pigeon = new PigeonIMU(talons[3]);
        }

        public static void Main()
        {
            //while (true)
            //{
            //    float[] vals = new float[4];
            //    pigeon.GetAccelerometerAngles(vals);
            //    Debug.Print(Utils.ArrToString(vals) + " ");
            //}
            Serial.readFromSerial(talons); // for serial control
            //Gamepad.handleXGamepad(talons); // for direct control
        }
    }
}
