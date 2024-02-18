using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using HERO_Serial;
using System;
using Microsoft.SPOT;

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
	private bool enable;

	private Drivetrain()
	{
		// This is a singleton
		// All the talons for the drivetrain live here
		// Have an init, then several drive functions
		// All the talons on one side will follow a leader talon
		
		// Initalize all the Talons
		leftLeader = new TalonSRX((int)Constants.CANID.DRIVETRAIN_FRONT_LEFT_TALON_ID);
		leftFollower = new TalonSRX((int)Constants.CANID.DRIVETRAIN_BACK_LEFT_TALON_ID);
		rightLeader = new TalonSRX((int)Constants.CANID.DRIVETRAIN_FRONT_RIGHT_TALON_ID);
		rightFollower = new TalonSRX((int)Constants.CANID.DRIVETRAIN_BACK_RIGHT_TALON_ID);


		// Set the followers to follow the leader
		leftFollower.Follow(leftLeader);
		rightFollower.Follow(rightLeader);

		// TODO: Add settings, current limits, etc. 
		leftLeader.SetInverted(true);
		leftFollower.SetInverted(true);
		rightLeader.SetInverted(false);
		rightFollower.SetInverted(false);

		// Put in brake mode
		leftLeader.SetNeutralMode(NeutralMode.Brake);
		leftLeader.SetNeutralMode(NeutralMode.Brake);

		enable = true;

	}

	public static Drivetrain getInstance()
    {
		if (instance == null){
			instance = new Drivetrain();
        }

		return instance;
    }

	public float[] GetCurrents(PowerDistributionPanel pdp)
    {
		float[] currents = new float[4];
		currents[0] = pdp.GetChannelCurrent((int)Constants.CANID.DRIVETRAIN_FRONT_LEFT_TALON_ID); // this might be printing pdp error?
		currents[1] = pdp.GetChannelCurrent((int)Constants.CANID.DRIVETRAIN_BACK_LEFT_TALON_ID);
		currents[2] = pdp.GetChannelCurrent((int)Constants.CANID.DRIVETRAIN_FRONT_RIGHT_TALON_ID);
		currents[3] = pdp.GetChannelCurrent((int)Constants.CANID.DRIVETRAIN_BACK_RIGHT_TALON_ID);
		//currents[0] = leftLeader.GetOutputCurrent();
		//currents[1] = leftFollower.GetOutputCurrent();
		//currents[2] = rightLeader.GetOutputCurrent();
		//currents[3] = rightFollower.GetOutputCurrent();


		// for motor current tests
		Debug.Print("CURRENT: " + currents[0].ToString());
		Debug.Print("CURRENT: " + currents[1].ToString());
		Debug.Print("CURRENT: " + currents[2].ToString());
		Debug.Print("CURRENT: " + currents[3].ToString());
		Debug.Print("PDP VOLTAGE: " + pdp.GetVoltage().ToString());






		return currents;
    }

	// Quick function to stop all the motors
	public void Stop()
    {
		DirectDrive(0.0f, 0.0f, 0.0f);
		// I think this will disable those motors. May need to explicitly enabled
		leftLeader.Set(ControlMode.PercentOutput, 0.0f);
		rightLeader.Set(ControlMode.PercentOutput, 0.0f);
		// Maybe does something? 
		leftLeader.Set(ControlMode.Disabled, 0.0f);
		leftLeader.Set(ControlMode.Disabled, 0.0f);
		enable = false;
	}

	// Percent Output drive mode. Takes a percent forwards (1 is max speed forwards, -1 max speed reverse)
	// and a turn (1 is turn clockwise, -1 turn counter-clockwise)
	// Designed such that controller input can be passed directly to this function
	public void DirectDrive(float forward, float turn, float upperBound)
    {
		DirectDriveLeft(forward + turn, upperBound);
		DirectDriveRight(forward - turn, upperBound);
		//leftLeader.Set(ControlMode.PercentOutput, Utils.thresh(forward + turn, upperBound));
		//rightLeader.Set(ControlMode.PercentOutput, Utils.thresh(forward - turn, upperBound));
	}

	public void DirectDriveLeft(float power, float upperBound)
    {
		if (enable) {
			leftLeader.Set(ControlMode.PercentOutput, 0.5);

			// test code to identify correct Talon:
			//leftLeader.Set(ControlMode.PercentOutput, 0.5); // the talons with IDs 0 and 2 should light up
		}
		
    }

	public void DirectDriveRight(float power, float upperBound)
	{
		if (enable)
        {
			rightLeader.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
		}
		
	}
}
