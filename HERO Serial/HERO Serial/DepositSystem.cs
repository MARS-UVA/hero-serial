using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using HERO_Serial;
using System;

/**
 * This is a class to represent the Deposit subsystem. It includes the basket and conviour system (if that happens)
 * It conatins the motors controllers, their configurations, and functions to control them
 * This is a singleton, and can be referenced anywhere
*/
class DepositSystem
{
    private static DepositSystem instance;
    private readonly TalonSRX basketLifter;

    private DepositSystem()
    {
        basketLifter = new TalonSRX((int)Constants.CANID.DEPOSITSYSTEM_BASKET_LIFTER_TALON_ID);
    }

    public static DepositSystem getInstance()
    {
        if (instance == null)
        {
            instance = new DepositSystem();
        }

        return instance;
    }

    // Quick function to stop all the motors
    public void Stop()
    {
        BasketLiftDirectControl(0.0f, 0.0f);
        // I think this will disable those motors. May need to explicitly enabled
        basketLifter.Set(ControlMode.Disabled, 0.0f);
    }

    public void BasketLiftDirectControl(float power, float upperBound)
    {
        basketLifter.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
    }
}