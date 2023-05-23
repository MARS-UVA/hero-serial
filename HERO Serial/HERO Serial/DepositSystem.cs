using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using HERO_Serial;
using CTRE.Phoenix;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT;
using System;

/**
 * This is a class to represent the Deposit subsystem. It includes the basket and conveyor system (if that happens)
 * It conatins the motors controllers, their configurations, and functions to control them
 * This is a singleton, and can be referenced anywhere
*/
class DepositSystem
{
    private static DepositSystem instance;
    private readonly TalonSRX basketLifter0;
    private readonly TalonSRX basketLifter1;
    private readonly TalonSRX basketFlipper;
    private bool enable;
    static readonly InputPort topSwitch = new InputPort(CTRE.HERO.IO.Port6.Pin4, false, Port.ResistorMode.Disabled);
    static readonly InputPort botSwitch = new InputPort(CTRE.HERO.IO.Port6.Pin3, false, Port.ResistorMode.Disabled);

    private DepositSystem()
    {
        basketLifter0 = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER0_TALON_ID);
        basketLifter1 = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER1_TALON_ID);
        basketFlipper = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_FLIP_TALON_ID);
        enable = true;

        basketFlipper.ConfigSelectedFeedbackSensor(FeedbackDevice.QuadEncoder);
        // basketFlipper.ConfigVelocityMeasurementPeriod(VelocityMeasPeriod.Period_25Ms);
        // basketFlipper.ConfigVelocityMeasurementWindow(16);
    }

    public static DepositSystem getInstance()
    {
        if (instance == null)
        {
            instance = new DepositSystem();
        }

        return instance;
    }

    public float[] GetCurrents(PowerDistributionPanel pdp)
    {
        float[] currents = new float[3];
        currents[0] = pdp.GetChannelCurrent((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER0_TALON_ID);
        currents[1] = pdp.GetChannelCurrent((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER1_TALON_ID);
        currents[2] = pdp.GetChannelCurrent((int)Constants.CANID.DEPOSITSYSTEM_BASKET_FLIP_TALON_ID);
        //currents[0] = basketLifter.GetOutputCurrent();

        return currents;
    }

    // Get the values on the deposit bin limit switches; 1 = pressed, 0 = not pressed
    public byte[] GetSwitches()
    {
        byte[] switches = new byte[2]; // top, bottom
        if (topSwitch.Read())
        {
            switches[0] = (byte)1;
        } 
        else
        {
            switches[0] = (byte)0;
        }
        if (botSwitch.Read())
        {
            switches[1] = (byte)1;
        }
        else
        {
            switches[1] = (byte)0;
        }
        return switches;
    }

    public void PrintEncoderValues()
    {
        Debug.Print("Encoder Velocity = " + basketFlipper.GetSelectedSensorVelocity());
        Debug.Print("Encoder Position = " + basketFlipper.GetSelectedSensorPosition());
        Debug.Print("Commanded Output = " + basketFlipper.GetMotorOutputPercent());
    }

    // Quick function to stop all the motors
    public void Stop()
    {
        BasketLiftDirectControl(0.0f, 0.0f);
        // I think this will disable those motors. May need to explicitly enabled
        basketLifter0.Set(ControlMode.Disabled, 0.0f);
        basketLifter1.Set(ControlMode.Disabled, 0.0f);
        basketFlipper.Set(ControlMode.Disabled, 0.0f);

        enable = false;
    }

    public void BasketLiftDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            basketLifter0.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
            basketLifter1.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
        
    }

    public void FlipperDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            basketFlipper.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
    }
}