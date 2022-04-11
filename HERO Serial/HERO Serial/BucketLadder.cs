using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using HERO_Serial;

/**
 * This is a class to represent the BucketLadder subsystem
 * This does NOT currently include the basket or conviour belt systems
 * It conatins the motors controllers, their configurations, and functions to control them
 * This is a singleton, and can be referenced anywhere
 */
public class BucketLadder
{
    private static BucketLadder instance;
    private readonly TalonSRX ladderLifter;
    private readonly TalonSRX ladderExtender;
    private readonly TalonSRX chainDriver;
    private bool enable;


    private BucketLadder()
    {

        // Initalize all the Talons
        ladderLifter = new TalonSRX((int)Constants.CANID.BUCKETLADDER_LIFTER_TALON_ID); // This is wired to 2 different motors. Not sure if that's a good idea
        ladderExtender = new TalonSRX((int)Constants.CANID.BUCKETLADDER_EXTENDER_TALON_ID);
        chainDriver = new TalonSRX((int)Constants.CANID.BUCKETLADDER_CHAIN_DRIVER_TALON_ID);

        // TODO: Add settings, current limits, etc
        enable = true;

    }

    public static BucketLadder getInstance()
    {
        if (instance == null)
        {
            instance = new BucketLadder();
        }

        return instance;
    }

    public float[] GetCurrents(PowerDistributionPanel pdp)
    {
        float[] currents = new float[3];
        currents[0] = pdp.GetChannelCurrent((int)Constants.CANID.BUCKETLADDER_LIFTER_TALON_ID);
        currents[1] = pdp.GetChannelCurrent((int)Constants.CANID.BUCKETLADDER_EXTENDER_TALON_ID);
        currents[2] = pdp.GetChannelCurrent((int)Constants.CANID.BUCKETLADDER_CHAIN_DRIVER_TALON_ID);
        //currents[0] = ladderLifter.GetOutputCurrent();
        //currents[1] = ladderExtender.GetOutputCurrent();
        //currents[2] = chainDriver.GetOutputCurrent();

        return currents;
    }

    // Quick function to stop all the motors
    public void Stop()
    {
        ExtendDirectControl(0.0f, 0.0f);
        HeightDirectControl(0.0f, 0.0f);
        // I think this will disable those motors. May need to explicitly enabled
        ladderLifter.Set(ControlMode.Disabled, 0.0f);
        ladderExtender.Set(ControlMode.Disabled, 0.0f);
        chainDriver.Set(ControlMode.Disabled, 0.0f);

        enable = false;
    }


    // Gives a percent output to the extension motor
    public void ExtendDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            ladderExtender.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
        
    }

    // Gives a percent output to the height control motor(s)
    public void HeightDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            ladderLifter.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
        
    }

    // Gives a percent output to the chain control motor(s)
    public void ChainDirectControl(float power, float upperBound)
    {
        if (enable)
        {
            chainDriver.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
        }
        
    }
}