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
        const bool DIRECT_DRIVE_ENABLED = false;

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

            //analoginput analoginput0 = new analoginput(ctre.hero.io.port8.analog_pin3);

            //System.IO.Ports.SerialPort _uart = new System.IO.Ports.SerialPort(CTRE.HERO.IO.Port1.UART, 115200);

            // IMU testing
            byte DeviceAddress = 0x68 >> 1;
            int ClockRate = 100; // clock rate in kHz, 100kHz is the standard for I2C
            IMUModule IMU1 = new IMUModule(DeviceAddress, ClockRate);

            //var gamepad = new LogitechGamepad(0);


            while (true)
            {
                if (DIRECT_DRIVE_ENABLED)
                {
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
                    //double read0 = analogInput0.Read();
                    //Debug.Print("Analog output: ");
                    //Debug.Print(read0.ToString());

                    //byte[] toWrite = new byte[8];
                    //toWrite[0] = 54;
                    //toWrite[1] = 54;
                    //toWrite[2] = 54;
                    //toWrite[3] = 54;
                    //toWrite[4] = 54;
                    //toWrite[5] = 54;
                    //toWrite[6] = 54;
                    //toWrite[7] = 54;

                    //_uart.Write(toWrite, 0, 8);

                    //IMU testing
                    //uint[] gyroData = IMU1.ReadGyroscopeData();
                    //for (int i = 0; i < gyroData.Length; i++)
                    //{
                    //    Debug.Print("Gyroscope Data: (X, Y, Z)");
                    //    Debug.Print(gyroData[i].ToString() + ", ");
                    //}
                    int address = IMU1.readAddress();
                    Debug.Print("Address: " + address.ToString());


                    serial.ReadFromSerial();
                    control.ReadAction(serial.decoded);
                    control.GetStatus();
                    serial.SendBytes(control.dataOut);
                }
            }
            
        }
    }
}
