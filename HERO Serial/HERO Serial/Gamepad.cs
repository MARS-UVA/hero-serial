using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using Microsoft.SPOT;
using System;

/**
 * This is a wrapper class for the Logitech Wireless Gamepad controller. 
 * This is simply to give each button and axis its own function so things are more readable
 */
class Gamepad
{

    private readonly GameController gamepad;

    public Gamepad(uint usbIndex)
    {
        // It would seem that UsbHostDevice and GameController are undocumented
        var instance = UsbHostDevice.GetInstance(usbIndex);
        instance.SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);

        gamepad = new GameController(instance);
    }

    public bool IsConnected()
    {
        return gamepad.GetConnectionStatus() == UsbDeviceConnection.Connected;
    }

    // TODO: Check these are using the correct axis indecies
    public float GetRightX()
    {
        return gamepad.GetAxis(0); 
    }
    public float GetRightY()
    {
        return gamepad.GetAxis(1);
    }
    public float GetLeftX()
    {
        return gamepad.GetAxis(2);
    }
    public float GetLeftY()
    {
        return gamepad.GetAxis(3);
    }

    public float[] GetRightStick()
    {
        return new float[2] { GetRightX(), GetRightY() };
    }

    public float[] GetLeftStick()
    {
        return new float[2] { GetLeftX(), GetLeftY() };
    }

    // TODO: Confirm what button index this is
    public bool XIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool YIsPressed()
    {
        return gamepad.GetButton(5);
    }
    // TODO: Confirm what button index this is
    public bool AIsPressed()
    {
        return gamepad.GetButton(1);
    }
    // TODO: Confirm what button index this is
    public bool BIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool DPadNorthIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool DPadSouthIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool DPadWestIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool DPadEastIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool LeftTriggerIsPressed()
    {
        return gamepad.GetButton(0);
    }
    // TODO: Confirm what button index this is
    public bool RightTriggerIsPressed()
    {
        return gamepad.GetButton(0);
    }


}

