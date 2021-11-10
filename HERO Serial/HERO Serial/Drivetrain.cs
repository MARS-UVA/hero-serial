using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;


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

		leftLeader = new TalonSRX(4);
		leftFollower = new TalonSRX(5);
		rightLeader = new TalonSRX(6);
		rightFollower = new TalonSRX(7);



	}

	public static Drivetrain getInstance()
    {
		if (instance == null){
			instance = new Drivetrain();
        }

		return instance;
    }
}
