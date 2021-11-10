using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;


public class Drivetrain

	private static readonly Drivetrain instance;
	private static readonly TalonSRX leftLeader;
{
	private Drivetrain()
	{
		// This is a singleton
		// All the talons for the drivetrain live here
		// Have an init, then several drive functions
		// All the talons on one side will follow a leader talon

		leftLeader = new TalonSRX();
		leftFollower = new TalonSRX();
		fdsklje


	}

	public Drivetrain getInstance()
    {
		if this.instance == null{
			this.instance = new Drivetrain();
        }

		return this.instance;
    }
}
