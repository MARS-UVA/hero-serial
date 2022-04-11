using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using HERO_Serial;
using CTRE.Phoenix;
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
    private readonly TalonSRX conveyorDriver;
    private bool enable;

    private DepositSystem()
    {
        basketLifter0 = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER0_TALON_ID);
        basketLifter1 = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER1_TALON_ID);
        conveyorDriver = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_CONVEYOR_DRIVER_TALON_ID);
        enable = true;
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
        currents[2] = pdp.GetChannelCurrent((int)Constants.CANID.DEPOSITSYSTEM_CONVEYOR_DRIVER_TALON_ID);
        //currents[0] = basketLifter.GetOutputCurrent();

        return currents;
    }

    // Quick function to stop all the motors
    public void Stop()
    {
        BasketLiftDirectControl(0.0f, 0.0f);
        // I think this will disable those motors. May need to explicitly enabled
        basketLifter0.Set(ControlMode.Disabled, 0.0f);
        basketLifter1.Set(ControlMode.Disabled, 0.0f);
        conveyorDriver.Set(ControlMode.Disabled, 0.0f);

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

    public void ConveyorDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            conveyorDriver.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
    }
}