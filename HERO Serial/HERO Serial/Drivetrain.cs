using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;

/**
 * This is a class to represent the Drivetrain subsystem
 * It conatins the motors controllers, their configurations, and functions to control them
 * This is a singleton, and can be referenced anywhere
 */
public class Drivetrain
{ 
	private static Drivetrain instance;
	private readonly TalonSRX leftLeader;
	private readonly TalonSRX leftFollower;
	private readonly TalonSRX rightLeader;
	private readonly TalonSRX rightFollower;

	private Drivetrain()
	{
		// This is a singleton
		// All the talons for the drivetrain live here
		// Have an init, then several drive functions
		// All the talons on one side will follow a leader talon
		
		// Initalize all the Talons
		leftLeader = new TalonSRX(4);
		leftFollower = new TalonSRX(5);
		rightLeader = new TalonSRX(6);
		rightFollower = new TalonSRX(7);


		// Set the followers to follow the leader
		leftFollower.Follow(leftLeader);
		rightFollower.Follow(rightLeader);

		// TODO: Add settings, current limits, etc. 
		leftLeader.SetInverted(true);


	}

	public static Drivetrain getInstance()
    {
		if (instance == null){
			instance = new Drivetrain();
        }

		return instance;
    }
}
