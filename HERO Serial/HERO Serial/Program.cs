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

            // Declaring pulse periods
            uint period = 10000; // period of pwm signal (in microseconds).
                                 // Sparkmax website specifies a frequency of 50-150Hz, so we used a period of 10ms
            uint duration = 2000; // pulse duration (in microseconds) (pulse width)

            // Change Port and Pin accordingly
            PWM sparkTest = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, period, duration, PWM.ScaleFactor.Microseconds, false); 
                // PWM.ScaleFactor.microseconds specifies the unit of time we're using

            // PWMSpeedController sparkMax = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin9);

            sparkTest.Start(); // starts the signal


            //var gamepad = new LogitechGamepad(0);

            while (true)
            {
                serial.ReadFromSerial();
                control.ReadAction(serial.decoded);
                control.GetStatus();
                serial.SendBytes(control.dataOut);

                sparkTest.Start(); // starts the signal

                // 100% reverse for 5s
                //sparkTest.Duration = 5000;
                //sparkTest.Period = 1000;


                // This function has no loop. Relies on this loop periodically execute. 
                // The old one had a while loop, so I'm not sure how it ever exited. 
                //control.DirectUserControl(); // New direct control function

                /*
                 * Y - Raises the basket
                 * A - Lowers the basket
                 * Right stick should move the drivetrain
                 */

                //Thread.Sleep(10);
            }
            
        }
    }
}
