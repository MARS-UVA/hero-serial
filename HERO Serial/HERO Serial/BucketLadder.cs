using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;

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

    public BucketLadder getInstance()
    {
        if (instance == null)
        {
            instance = new BucketLadder();
        }

        return instance;
    }
}