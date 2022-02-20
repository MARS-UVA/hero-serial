﻿using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using HERO_Serial;

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

	}

	public static Drivetrain getInstance()
    {
		if (instance == null){
			instance = new Drivetrain();
        }

		return instance;
    }

	public float[] GetCurrents()
    {
		float[] currents = new float[4];
		currents[0] = leftLeader.GetOutputCurrent();
		currents[1] = leftFollower.GetOutputCurrent();
		currents[2] = rightLeader.GetOutputCurrent();
		currents[3] = rightFollower.GetOutputCurrent();

		return currents;
    }

	// Quick function to stop all the motors
	public void Stop()
    {
		DirectDrive(0.0f, 0.0f, 0.0f);
		// I think this will disable those motors. May need to explicitly enabled
		leftLeader.Set(ControlMode.PercentOutput, 0.0f);
		rightLeader.Set(ControlMode.PercentOutput, 0.0f);
	}

	// Percent Output drive mode. Takes a percent forwards (1 is max speed forwards, -1 max speed reverse)
	// and a turn (1 is turn clockwise, -1 turn counter-clockwise)
	// Designed such that controller input can be passed directly to this function
	public void DirectDrive(float forward, float turn, float upperBound)
    {
		leftLeader.Set(ControlMode.PercentOutput, Utils.thresh(forward + turn, upperBound));
		rightLeader.Set(ControlMode.PercentOutput, Utils.thresh(forward - turn, upperBound));
	}

	public void DirectDriveLeft(float power, float upperBound)
    {
		leftLeader.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
    }

	public void DirectDriveRight(float power, float upperBound)
	{
		rightLeader.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
	}
}
