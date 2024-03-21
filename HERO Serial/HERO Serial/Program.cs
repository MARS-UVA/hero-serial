using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace HERO_Serial
{
    public class Program
    {
        static readonly TalonSRX[] talons = new TalonSRX[8];

        static Program() {
            // New IDS:
            // 4-5: left wheels
            // 6-7: right wheels
            // 8: bucket ladder angle, two motors
            // 9: bucket ladder translation
            // 10: chain driver (not attached)?
            // 11: deposit bin angle, two motors
            // 12: deposit bin gate (not attached)
            //int[] talonIdx = { 4, 5, 6, 7, 8, 9, 10, 11 };
            //bool[] inverted = { true, true, false, false, false, false, false, false };
            //for (int i = 0; i < talonIdx.Length; i++)
            //{
            //    var t = new TalonSRX(talonIdx[i]);
            //    t.SetInverted(inverted[i]);
            //    talons[i] = t;

            //     // this is garbage
            //     t.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Relative, 0);
            //     t.GetSelectedSensorVelocity(0);
            // }
            // pigeon = new PigeonIMU(talons[7]);
        }

        public static void Main()
        {
            //while (true)
            //{
            //    float[] vals = new float[4];
            //    pigeon.GetAccelerometerAngles(vals);
            //    Debug.Print(Utils.ArrToString(vals) + " ");
            //}
            //var control = new Control(talons);
            var control = new Control();
            var serial = new Serial();

            //var gamepad = new LogitechGamepad(0);

            // Electrical Test Code for  GPIO Input Port: port 3 pin 5 (middle right port on HERO Hat)
            InputPort digitalIn1 = new InputPort(CTRE.HERO.IO.Port3.Pin5, false, Port.ResistorMode.Disabled);

            // port 3 pin 9(top right port on HERO Hat)
            OutputPort digitalOut1 = new OutputPort(CTRE.HERO.IO.Port3.Pin9, false);



            while (true)
            {
                bool isTestDrive = digitalIn1.Read();
                if (isTestDrive)
                {
                     digitalOut1.Write(true); //turn LED on

                    /*
                     * Y - Raises the basket
                     * A - Lowers the basket
                     * Right stick should move the drivetrain
                     */
                    control.DirectUserControl(); // Direct control function

                    Thread.Sleep(10);
                }
                else
                {
                    digitalOut1.Write(false); // turn LED off

                    serial.ReadFromSerial();
                    control.ReadAction(serial.decoded);
                    control.GetStatus();
                    serial.SendBytes(control.dataOut);
                }
            }
            
        }
    }
}
