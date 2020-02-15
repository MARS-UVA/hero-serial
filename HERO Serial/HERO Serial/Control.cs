using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using System;
using System.Threading;

namespace HERO_Serial
{
    class Control
    {
        readonly TalonSRX[] talons;
        readonly byte[] current;
        readonly byte[] temp = new byte[6 * 4];
        readonly float[] linear_vel;
        readonly float[] angular_vel;

        public Control(TalonSRX[] talons)
        {
            this.talons = talons;
            current = new byte[talons.Length];
        }

        public void HandleXGamepad()
        {
            UsbHostDevice.GetInstance(0).SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);
            var myGamepad = new GameController(UsbHostDevice.GetInstance(0));
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

                    Watchdog.Feed();
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
                int count = decoded[0]; // length prefixed
                if (count == talons.Length) // direct motor output
                {
                    for (int j = 0; j < count; j++)
                    {
                        float val = decoded[j + 1];
                        val = (val - 100) / 100;
                        talons[j].Set(ControlMode.PercentOutput, val);
                    }
                }
                else if (count == 6 * 4)
                {

                    for (int j = 0; j < 24; j += 4)
                        temp[j] = decoded[j + 1];

                    for (int j = 0; j < 3; j++)
                        linear_vel[j] = BitConverter.ToSingle(temp, j * 4);

                    for (int j = 0; j < 3; j++)
                        angular_vel[j] = BitConverter.ToSingle(temp, (j + 3) * 4);

                    // TODO
                    // Convert linear vel and angular vel
                    // PID control
                }
                decoded.RemoveFront(count + 1); // remove count and data bytes
            }
        }

        public byte[] GetMotorCurrent()
        {
            for (int i = 0; i < talons.Length; i++)
            {
                current[i] = (byte)(talons[i].GetOutputCurrent() * 4);
            }
            return current;
        }
    }
}
