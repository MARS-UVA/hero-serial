using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using Microsoft.SPOT;

/**
 * This is a wrapper class for the Logitech Wireless Gamepad controller. 
 * This is simply to give each button and axis its own function so things are more readable
 */
class LogitechGamepad
{

    private readonly GameController gamepad;

    public LogitechGamepad(uint usbIndex)
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
    public float GetLeftTrigger()
    {
        return gamepad.GetAxis(4);
    }
    public float GetRightTrigger()
    {
        return gamepad.GetAxis(5);
    }
    public float GetRightX()
    {
        return gamepad.GetAxis(2); 
    }
    public float GetRightY()
    {
        return gamepad.GetAxis(3);
    }
    public float GetLeftX()
    {
        return gamepad.GetAxis(0);
    }
    public float GetLeftY()
    {
        return gamepad.GetAxis(1);
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
    public bool IsXPressed()
    {
        return gamepad.GetButton(3);
    }
    // TODO: Confirm what button index this is
    public bool IsYPressed()
    {
        return gamepad.GetButton(4);
    }
    // TODO: Confirm what button index this is
    public bool IsAPressed()
    {
        return gamepad.GetButton(1);
    }
    // TODO: Confirm what button index this is
    public bool IsBPressed()
    {
        return gamepad.GetButton(2);
    }
    
    // TODO: Confirm what button index this is
    public bool IsLeftShoulderPressed()
    {
        return gamepad.GetButton(5);
    }
    // TODO: Confirm what button index this is
    public bool IsRightShoulderPressed()
    {
        return gamepad.GetButton(6);
    }

    public bool IsBackPressed()
    {
        return gamepad.GetButton(7);
    }

    public bool IsStartPressed()
    {
        return gamepad.GetButton(8);
    }

    public bool IsLogitechButtonPressed()
    {
        return gamepad.GetButton(11);
    }

    public bool IsLeftStickPressed()  
    {
        return gamepad.GetButton(9);
    }

    public bool IsRightStickPressed() 
    {
        return gamepad.GetButton(10);
    }


    
}

