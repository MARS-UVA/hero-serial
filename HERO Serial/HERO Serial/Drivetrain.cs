using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using HERO_Serial;
using System.Threading;
using Microsoft.SPOT;


/**
 * This is a class to represent the Drivetrain subsystem
 * It conatins the motors controllers, their configurations, and functions to control them
 * This is a singleton, and can be referenced anywhere
 */
public class Drivetrain
{
	private static Drivetrain instance;

	// Motor controllers for the drivetrain (two per side: leader and follower).
	private readonly TalonSRX leftLeader, leftFollower, rightFollower, rightLeader;

	// Control flags and variables.
	private bool enable; // Flag to enable or disable
	private float prevLeftPower, prevRightPower; // Store previous power levels


	private int arrayLen = 200;
	private float[] prevW1Currents, prevW2Currents, prevW3Currents, prevW4Currents;

	// Sums of currents for each motor 
	private float W1Sum = 0, W2Sum = 0, W3Sum = 0, W4Sum = 0;


	private int currentIter = 0, maxCurrent = 80, maxTotalCurrent = 130;

	private float[] prevBLCurrents;
	private float BLSum = 0;

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

		// Configure a specific motor's inverse, brake mode, seconds from neutral to full output in drivetrain
		this.configureMotor(leftLeader, true, NeutralMode.Brake, 5f);
		this.configureMotor(leftFollower, true, NeutralMode.Brake, 5f);
		this.configureMotor(rightLeader, false, NeutralMode.Brake, 5f);
		this.configureMotor(rightFollower, false, NeutralMode.Brake, 5f);

		enable = true;
		//notStallStartTime = 0;
		prevW1Currents = new float[arrayLen];
		prevW2Currents = new float[arrayLen];
		prevW3Currents = new float[arrayLen];
		prevW4Currents = new float[arrayLen];
		prevBLCurrents = new float[arrayLen];
	}

	public void configureMotor(TalonSRX talonMotor, bool inverted, NeutralMode neutralMode, float secondsFromNeutralToFull)
	{
		// Set inverted
		talonMotor.SetInverted(inverted);
		// Used to put motors in brake mode
		talonMotor.SetNeutralMode(neutralMode);
		// ____ seconds from neutral to full output (during open-loop control)
		talonMotor.ConfigOpenloopRamp(secondsFromNeutralToFull);
	}

	public static Drivetrain getInstance()
	{
		if (instance == null)
		{
			instance = new Drivetrain();
		}

		return instance;
	}

	public float[] GetCurrents(PowerDistributionPanel pdp)
	{
		float[] currents = new float[4];
		currents[0] = pdp.GetChannelCurrent(12); // front left
		currents[1] = pdp.GetChannelCurrent(13); // front right
		currents[2] = pdp.GetChannelCurrent(2); // back left
		currents[3] = pdp.GetChannelCurrent(3); // back right

		//currents[0] = 10f;
		//currents[1] = 20f;

		float bucketladderCurrent = pdp.GetChannelCurrent(4);

		currentIter += 1;
		if (currentIter == arrayLen)
		{
			currentIter = 0;
		}

		W1Sum += (currents[0] - prevW1Currents[currentIter]);
		W2Sum += (currents[1] - prevW2Currents[currentIter]);
		W3Sum += (currents[2] - prevW3Currents[currentIter]);
		W4Sum += (currents[3] - prevW4Currents[currentIter]);
		prevW1Currents[currentIter] = currents[0];
		prevW2Currents[currentIter] = currents[1];
		prevW3Currents[currentIter] = currents[2];
		prevW4Currents[currentIter] = currents[3];

		BLSum += (bucketladderCurrent - prevBLCurrents[currentIter]);
		prevBLCurrents[currentIter] = bucketladderCurrent;

		// if avg current for any of the wheels > maxCurrent, stop robot's drivetrain
		if (W1Sum > maxCurrent * arrayLen || W2Sum > maxCurrent * arrayLen || W3Sum > maxCurrent * arrayLen || W4Sum > maxCurrent * arrayLen)
		{
			// stop everything, and wait for 2 seconds to reset
			Stop();
			Debug.Print("STOPPING");
			Thread.Sleep(2000);
			enable = true;

			// reset sum values
			W1Sum = 0;
			W2Sum = 0;
			W3Sum = 0;
			W4Sum = 0;
			prevW1Currents = new float[arrayLen];
			prevW2Currents = new float[arrayLen];
			prevW3Currents = new float[arrayLen];
			prevW4Currents = new float[arrayLen];

		}
		// if avg current of all motors exceed maxTotalCurrent, shut down robot's drivetrain
		else if ((W1Sum + W2Sum + W3Sum + W4Sum + BLSum) > maxTotalCurrent * arrayLen)
		{
			Stop();
			Thread.Sleep(2000);
			enable = true;

			// reset sum values
			W1Sum = 0;
			W2Sum = 0;
			W3Sum = 0;
			W4Sum = 0;
			prevW1Currents = new float[arrayLen];
			prevW2Currents = new float[arrayLen];
			prevW3Currents = new float[arrayLen];
			prevW4Currents = new float[arrayLen];
			BLSum = 0;
			prevBLCurrents = new float[arrayLen];

		}

		return currents;
	}

	public float[] GetAvgCurrents()
	{
		float[] avgCurrents = new float[] { W1Sum / arrayLen, W2Sum / arrayLen, W3Sum / arrayLen, W4Sum / arrayLen, BLSum / arrayLen };
		// Debug.Print(avgCurrents[0].ToString());
		return avgCurrents;
	}

	// Quick function to stop all the motors
	public void Stop()
	{
		DirectDrive(0.0f, 0.0f, 0.0f);
		// I think this will disable those motors. May need to explicitly enabled
		leftLeader.Set(ControlMode.PercentOutput, 0.0f);
		rightLeader.Set(ControlMode.PercentOutput, 0.0f);
		// Maybe does something? 
		//leftLeader.Set(ControlMode.Disabled, 0.0f);
		//leftLeader.Set(ControlMode.Disabled, 0.0f);
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
		if (System.Math.Abs(power) < System.Math.Abs(prevLeftPower)) // when decelerating, ramp down speed immediately (within 0.1s)
		{
			leftLeader.ConfigOpenloopRamp(0.1f);
			leftFollower.ConfigOpenloopRamp(0.1f);
		}
		else if (System.Math.Abs(power) > System.Math.Abs(prevLeftPower)) // when accelerating, ramp up speed gradually (0.5s)
		{
			leftLeader.ConfigOpenloopRamp(0.5f);
			leftFollower.ConfigOpenloopRamp(0.5f);
		}
		if (enable)
		{
			leftLeader.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
			prevLeftPower = power;
		}

	}

	public void DirectDriveRight(float power, float upperBound)
	{
		if (System.Math.Abs(power) < System.Math.Abs(prevRightPower)) // when decelerating, ramp down speed immediately (within 0.1s)
		{
			rightLeader.ConfigOpenloopRamp(0.1f);
			rightFollower.ConfigOpenloopRamp(0.1f);
		}
		else if (System.Math.Abs(power) > System.Math.Abs(prevRightPower)) // when accelerating, ramp up speed gradually (0.5s)
		{
			rightLeader.ConfigOpenloopRamp(0.5f);
			rightFollower.ConfigOpenloopRamp(0.5f);
		}
		if (enable)
		{
			rightLeader.Set(ControlMode.PercentOutput, Utils.thresh(power, upperBound));
			prevRightPower = power;
		}

	}
}