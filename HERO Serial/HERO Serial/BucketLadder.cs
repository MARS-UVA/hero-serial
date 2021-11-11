using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
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


    private BucketLadder()
    {

        // Initalize all the Talons
        ladderLifter = new TalonSRX(8); // This is wired to 2 different motors. Not sure if that's a good idea
        ladderExtender = new TalonSRX(9);
        chainDriver = new TalonSRX(10);

        // TODO: Add settings, current limits, etc

    }

    public static BucketLadder getInstance()
    {
        if (instance == null)
        {
            instance = new BucketLadder();
        }

        return instance;
    }

    // Gives a percent output to the extension motor
    public void ExtendDirectControl(float power, float upperBound)
    {
        ladderExtender.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
    }

    // Gives a percent output to the height control motor(s)
    public void HeightDirectControl(float power, float upperBound)
    {
        ladderLifter.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
    }
}