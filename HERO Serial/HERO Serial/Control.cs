using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;

namespace HERO_Serial
{
    class Control
    {
        readonly TalonSRX[] talons;
        
        public readonly byte[] dataOut;
        readonly byte[] temp = new byte[4 * 3];
        // linear x, linear y, angular z
        readonly float[] twist = new float[3];
        // arm angle potentiometer
        readonly AnalogInput pot1 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        // arm translation potentiometer
        readonly AnalogInput pot2 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);
        readonly int minAngle = 30;
        readonly int maxAngle = 90;
        readonly int minTrans = 1;
        readonly int maxTrans = 10;

        public Control(TalonSRX[] talons)
        {
            this.talons = talons;
            dataOut = new byte[talons.Length + 8];
        }

        public void HandleXGamepad()
        {
            var instance = UsbHostDevice.GetInstance(0);
            instance.SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);

            var myGamepad = new GameController(instance);
            var temp = new GameControllerValues();
            while (true)
            {
                if (myGamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    /* print the axis value */
                    myGamepad.GetAllValues(ref temp);
                    float rX = -temp.axes[2];
                    float rY = -temp.axes[3];

                    // different from DirectInput
                    talons[0].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talons[1].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talons[2].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));
                    talons[3].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));

                    talons[4].Set(ControlMode.PercentOutput, Utils.thresh(temp.axes[1], 0.1f));

                    uint buttons = temp.btns;
                    float depositBin = 0.0f;
                    if ((buttons & 8) != 0) // Y
                        depositBin = 1.0f;
                    else if ((buttons & 1) != 0) // A
                        depositBin = -1.0f;

                    float lt = temp.axes[4], rt = temp.axes[5];
                    talons[6].Set(ControlMode.PercentOutput, -(lt + 1) / 2 + (rt + 1) / 2);

                    talons[7].Set(ControlMode.PercentOutput, depositBin);

                    CTRE.Phoenix.Watchdog.Feed();
                    Thread.Sleep(10);
                    Debug.Print("axis:" + Utils.ArrToString(temp.axes));
                    Debug.Print("buttons: " + temp.btns);
                }
            }
        }

        public void ReadAction(RingBuffer decoded)
        {
            while (decoded.size > 0)
            {
                int count = decoded[0] & 0x3F; // length prefixed
                if (count == talons.Length) // direct motor output
                {
                    for (int j = 0; j < count; j++)
                    {
                        float val = decoded[j + 1];
                        val = (val - 100) / 100;
                        talons[j].Set(ControlMode.PercentOutput, val);
                    }
                }
                else if (count == 3 * 4) // if message length is 12 bytes (3 floats)
                {

                    for (int j = 1; j < 13; j += 4)
                        temp[j] = decoded[j];

                    for (int j = 0; j < 3; j++)
                        twist[j] = BitConverter.ToSingle(temp, j * 4);


                    // TODO
                    // Convert linear vel and angular vel
                    // PID control

                    // set arms and actuators to zero when in autonomy
                    for (int j = 4; j < 8; j++)
                        talons[j].Set(ControlMode.PercentOutput, 0.0f);
                }
                else if (count == 8) // message length is 8 bytes (2 floats) (arm angle and translation)
                {
                    for (int j = 1; j < 9; j += 4)
                        temp[j] = decoded[j];

                    float angleTarget = BitConverter.ToSingle(temp, 1);
                    float translationTarget = BitConverter.ToSingle(temp, 5);

                    // TODO convert pot voltages to a position
                    float angleNow = (float)pot1.Read();
                    float angleDiff = angleTarget - angleNow;
                    while (System.Math.Abs(angleDiff) < 10)
                    {
                        talons[4].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * System.Math.Sign(angleDiff));
                        angleNow = (float)pot1.Read();
                        angleDiff = angleTarget - angleNow;
                    }

                    float translationNow = (float)pot2.Read();
                    float translationDiff = translationTarget - translationNow;
                    while (System.Math.Abs(translationDiff) < 10)
                    {
                        talons[5].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(translationNow / translationTarget)) * System.Math.Sign(translationDiff));
                        translationNow = (float)pot2.Read();
                        translationDiff = translationTarget - translationNow;
                    }
                }
                decoded.RemoveFront(count + 1); // remove count and data bytes
            }
            CTRE.Phoenix.Watchdog.Feed();
        }

        public byte[] GetMotorCurrent()
        {
            for (int i = 0; i < talons.Length; i++)
                dataOut[i] = (byte)(talons[i].GetOutputCurrent() * 4);
            dataOut[talons.Length] = (byte)(pot1.Read() * 255);
            dataOut[talons.Length] = (byte)(pot2.Read() * 255);
            return dataOut;
        }

        public byte[] GetPotValue()
        {
            double val = pot1.Read();
            val = (maxAngle - minAngle) * val + minAngle;
            return BitConverter.GetBytes((float)val);
        }

        // get motor currents, arm angle, and arm translation and put into dataOut
        public void GetStatus()
        {
            double val;
            byte[] bytes;
            // motor currents
            for (int i = 0; i < talons.Length; i++)
                dataOut[i] = (byte)(talons[i].GetOutputCurrent() * 4);
            // arm angle
            val = pot1.Read();
            val = (maxAngle - minAngle) * val + minAngle; // convert to angle
            bytes = BitConverter.GetBytes((float)val); // convert to byte array
            for (int i = 0; i < bytes.Length; i++) // put in dataOut
                dataOut[i + talons.Length] = bytes[i];
            // arm translation
            val = pot2.Read();
            val = (maxTrans - minTrans) * val + minTrans; // convert to translation
            //bytes = BitConverter.GetBytes((float)val); // convert to byte array
            bytes = BitConverter.GetBytes((float)1.0); // dummy value bc no pot2 yet
            for (int i = 0; i < bytes.Length; i++) // put in dataOut
                dataOut[i + talons.Length + 4] = bytes[i];
        }
    }
}
