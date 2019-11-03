using Microsoft.SPOT;
using System.Threading;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix;
namespace HERO_Serial
{
    class Gamepad
    {
        public static void handleDGamepad(TalonSRX[] talon)
        {
            var myGamepad = new GameController(UsbHostDevice.GetInstance(0));
            var temp = new GameControllerValues();
            while (true)
            {
                if (myGamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    myGamepad.GetAllValues(ref temp);
                    float rX = temp.axes[2];
                    float rY = temp.axes[5];
                    talon[0].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));
                    talon[1].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));
                    talon[2].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talon[3].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));

                    uint buttons = temp.btns;
                    if ((buttons & 8) != 0) // Y
                        talon[4].Set(ControlMode.PercentOutput, 1.0f);
                    else if ((buttons & 2) != 0) // A
                        talon[4].Set(ControlMode.PercentOutput, -1.0f);
                    else
                        talon[4].Set(ControlMode.PercentOutput, 0.0f);
                    if ((buttons & 16) != 0) // LB
                        talon[5].Set(ControlMode.PercentOutput, 1.0f);
                    else if ((buttons & 32) != 0) // RB
                        talon[5].Set(ControlMode.PercentOutput, -1.0f);
                    else
                        talon[5].Set(ControlMode.PercentOutput, 0.0f);
                    talon[6].Set(ControlMode.PercentOutput, Utils.thresh(temp.axes[1], 0.1f));
                    Watchdog.Feed();
                    Thread.Sleep(20);
                    Debug.Print("axis:" + Utils.ArrToString(temp.axes));
                    Debug.Print("buttons: " + temp.btns);
                }
            }
        }
        public static void handleXGamepad(TalonSRX[] talon)
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
                    talon[0].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talon[1].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talon[2].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));
                    talon[3].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));

                    uint buttons = temp.btns;
                    if ((buttons & 8) != 0) // Y
                        talon[4].Set(ControlMode.PercentOutput, 1.0f);
                    else if ((buttons & 1) != 0) // A
                        talon[4].Set(ControlMode.PercentOutput, -1.0f);
                    else
                        talon[4].Set(ControlMode.PercentOutput, 0.0f);
                    float lt = temp.axes[4], rt = temp.axes[5];
                    talon[5].Set(ControlMode.PercentOutput, -(lt + 1) / 2 + (rt + 1) / 2);
                    talon[6].Set(ControlMode.PercentOutput, Utils.thresh(temp.axes[1], 0.1f));
                    Watchdog.Feed();
                    Thread.Sleep(20);
                    Debug.Print("axis:" + Utils.ArrToString(temp.axes));
                    Debug.Print("buttons: " + temp.btns);
                }
            }
        }
    }
}
