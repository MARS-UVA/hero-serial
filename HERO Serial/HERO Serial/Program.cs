using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Diagnostics;
using System.Threading;

namespace HERO_Serial
{
    public class Program
    {

        static Program() {
            // New IDS:
            // 0,2: left wheels
            // 13,16: right wheels
            // 14,15: bucket ladder lifters
            // 4: bucket ladder chain driver
            // 3: construction bin lifter
            // 10,11: no motor attached
        }

        public static void Main()
        {
            var control = new Control();
            var serial = new Serial();


            // GPIO input port for reading signal from switch: port 3 pin 5 (middle right port on HERO Hat)
            // switch: toggling near blue wire is production drive, orange wire side is direct drive
            InputPort digitalIn1 = new InputPort(CTRE.HERO.IO.Port3.Pin6, false, Port.ResistorMode.Disabled);

            // GPIO output port for turning on LED: 3 pin 9(top right port on HERO Hat)
            // LED is on for test drive, off for production drive
            //OutputPort digitalOut1 = new OutputPort(CTRE.HERO.IO.Port3.Pin9, false);

            var deposit = DepositSystem.getInstance();

            while (true)
            {
                bool isTestDrive = digitalIn1.Read(); // default is set to true by a pull-up resistor
                //bool isTestDrive = false;

                if (isTestDrive)
                {
                    //digitalOut1.Write(true); // turn LED on

                    /*
                     * Y - Raises construction bin
                     * A - Lowers construction bin
                     * Right stick should move the drivetrain
                     */
                    Debug.Print("");
                    control.DirectUserControl(); // Direct control function
                    Thread.Sleep(10);
                }
                else
                {
                    //deposit.IRServoDirectControl(270);
                    //deposit.WebcamServoDirectControl(0);

                    //digitalOut1.Write(false); // turn LED off


                    serial.ReadFromSerial();
                    control.ReadAction(serial.decoded);
                    control.GetStatus();

                    // todo: delete debug code 
                    //Debug.Print("length:" + control.dataOut.Length.ToString());
                    //for (int i = 0; i < control.dataOut.Length; i++)
                    //{
                    //    Debug.Print("Byte " + i + ": " + control.dataOut[i]);
                    //}

                    Debug.Print(control.dataOut[31].ToString());
                    serial.SendBytes(control.dataOut);
                }
            }
            
        }
    }
}
