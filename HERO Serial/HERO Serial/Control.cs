using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;

namespace HERO_Serial
{
    class Control
    {
        readonly TalonSRX[] talons;
        
        public readonly byte[] dataOut;
        readonly byte[] temp = new byte[4 * 3];
        // linear x, linear y, angular z
        readonly float[] twist = new float[3];
        // arm angle potentiometer
        readonly AnalogInput pot1 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        // arm translation potentiometer
        readonly AnalogInput pot2 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);
        readonly int minAngle = 30;
        readonly int maxAngle = 90;
        readonly int minTrans = 1;
        readonly int maxTrans = 10;

        public Control(TalonSRX[] talons)
        {
            this.talons = talons;
            dataOut = new byte[talons.Length + 8];
        }

        
        public void DirectUserControl()
        {
            var gamepad = new LogitechGamepad(0);

            if (gamepad.IsConnected())
            {

                // Get the subsystems
                var drivetrain = Drivetrain.getInstance();
                var bucketladder = BucketLadder.getInstance();
                var deposit = DepositSystem.getInstance();

                // Get drive input from the right stick
                float driveForwards = gamepad.GetRightX() * 0.25f;
                float driveTurn = gamepad.GetRightY() * 0.25f;

                // Pass it to the drivetrain
                drivetrain.DirectDrive(driveForwards, driveTurn, 0.1f);

                // Get input for the bucket ladder
                float bucketHeight = gamepad.GetLeftX();
                float bucketExtension = gamepad.GetLeftY();

                // Pass to the bucket ladder subsystem
                bucketladder.HeightDirectControl(bucketHeight, 0.1f);
                bucketladder.ExtendDirectControl(bucketExtension, 0.1f);

                // Get input for the basket
                // Y lifts the basket, X lowers it
                // This has been tested
                var basketLift = 0f;
                if (gamepad.IsYPressed() && gamepad.IsAPressed())
                {
                    basketLift = 0f;
                } 
                else if (gamepad.IsYPressed())
                {
                    basketLift = -1f;
                } 
                else if(gamepad.IsAPressed())
                {
                    basketLift = 1f;
                }

                // Pass it to the Deposit subsytem
                deposit.BasketLiftDirectControl(basketLift, 0.5f);

                // Feed the watchdog so we don't timeout
                CTRE.Phoenix.Watchdog.Feed();
            }
        }

        public void HandleXGamepad()
        {
            var instance = UsbHostDevice.GetInstance(0);
            instance.SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);

            var myGamepad = new GameController(instance);
            var temp = new GameControllerValues();
            while (true)
            {
                if (myGamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    /* print the axis value */
                    myGamepad.GetAllValues(ref temp);
                    float rX = -temp.axes[0];
                    float rY = -temp.axes[1];

                    // different from DirectInput
                    talons[0].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talons[1].Set(ControlMode.PercentOutput, Utils.thresh(rY + rX, 0.1f));
                    talons[2].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));
                    talons[3].Set(ControlMode.PercentOutput, Utils.thresh(rY - rX, 0.1f));

                    talons[4].Set(ControlMode.PercentOutput, Utils.thresh(temp.axes[2], 0.1f));
                    talons[5].Set(ControlMode.PercentOutput, Utils.thresh(temp.axes[3], 0.1f));

                    uint buttons = temp.btns;
                    float depositBin = 0.0f;
                    if ((buttons & 8) != 0) // Y
                        depositBin = 1.0f;
                    else if ((buttons & 1) != 0) // A
                        depositBin = -1.0f;

                    float lt = temp.axes[4], rt = temp.axes[5];
                    talons[6].Set(ControlMode.PercentOutput, -(lt + 1) / 2 + (rt + 1) / 2);

                    talons[7].Set(ControlMode.PercentOutput, depositBin);

                    CTRE.Phoenix.Watchdog.Feed();
                    Thread.Sleep(10);
                    Debug.Print("axis:" + Utils.ArrToString(temp.axes));
                    Debug.Print("buttons: " + temp.btns);
                }
            }
        }

        //changes the robot's motor output and arm angles to match target values specified by the laptop/jetson, which are stored in a RingBuffer
        public void ReadAction(RingBuffer decoded)
        {
            while (decoded.size > 0)
            {
                int count = decoded[0] & 0x3F; // length prefixed
                if (count == talons.Length) // if the message conveys direct motor output (1 value for each motor)
                {
                    for (int j = 0; j < count; j++) //sets each motor's percent output accordingly
                    {
                        float val = decoded[j + 1];
                        val = (val - 100) / 100;
                        talons[j].Set(ControlMode.PercentOutput, val);
                    }
                }
                else if (count == 3 * 4) // if message length is 12 bytes (3 floats), we must update the linear and angular velocity of the robot itself
                {

                    for (int j = 1; j < 13; j += 4) //stores the three values at indices 1, 5, and 9
                        temp[j] = decoded[j];

                    for (int j = 0; j < 3; j++)
                        twist[j] = BitConverter.ToSingle(temp, j * 4 + 1); //converts each value (linear vel x, linear vel y, angular vel about z) into a float. +1 added to fix the indices

                    // TODO:
                    // Adjust PID control to reflect pidgeon imu values and proper motors

                    //find differences b/w current and target values:
                    float currentAngularVel = (float)pot1.Read(); //WHERE WE READ THE VALUE FROM WILL CHANGE --> TODO: figure out how to read pidgeon imu values
                    float angularVelDiff = twist[2] - currentAngularVel; //twist 2 contains the target angular velocity


                    //keep moving the motor in the correct direction until the current velocities match the target values (within a small uncertainty)
                    //using a magnitude and direction approach: the robot's heading is first updated, and then once it's  facing the right diretion, it travels with the target velocity

                    // FIXME
                    float angularAccDiff = 0.0f;
                    float angleNow = 0.0f;
                    float angleTarget = 0.0f;
                    float linearXDiff = 0.0f;
                    float angleDiff = 0.0f;
                    //update heading
                    while (System.Math.Abs(angularAccDiff) > 10)
                    {
                        //to spin around z axis without translational motion, spin left wheels forward and right wheels backward with same magnitude of motor output
                        talons[0].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * System.Math.Sign(angleDiff)); //Math.Sign accounts for the direction, the Math.Max term sets the percent output magnitude with a minimun of 15%?
                        talons[1].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * System.Math.Sign(angleDiff));
                        talons[2].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * -1 * System.Math.Sign(angleDiff)); //factor of -1 changes the direction the motor spins
                        talons[3].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * -1 * System.Math.Sign(angleDiff));
                        currentAngularVel = (float)pot1.Read(); //TODO: update read in from pidgeon imu
                        angularVelDiff = twist[2] - currentAngularVel;
                    }
                    //update translational motion
                    
                    float currentLinearMag = (float)pot1.Read(); //gets a value for velocity from the pidgeon IMU (TODO)
                    float targetLinearMag = 0.0f; // FIXME // CTRE.Phoenix.Math.Sqrt(Math.Pow(twist[0], 2) + Math.Pow(twist[1], 2)); //magnitude of targe linear velocity (twist[0] is x component and twist[1] is y component)
                    float linearDiff = targetLinearMag - currentLinearMag;
                    while (System.Math.Abs(linearXDiff) > 10)
                    {
                        //talons 0 and 1 control left motor
                        //talons 2 and 3 control right motor

                        //send the robot forward:
                        for (int ind = 0; ind < 4; ind++) {
                        talons[ind].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(currentLinearMag / targetLinearMag)) * System.Math.Sign(linearDiff)); //Math.Sign accounts for the direction, the Math.Max term sets the percent output magnitude with a minimun of 15%?
                        }
                        currentLinearMag = (float)pot1.Read(); //TODO: update read in from pidgeon imu
                        linearDiff = targetLinearMag - currentLinearMag;
                    }


                    // set arms and actuators to zero when in autonomy
                    for (int j = 4; j < 8; j++)
                        talons[j].Set(ControlMode.PercentOutput, 0.0f);
                }
                else if (count == 8) // if message length is 8 bytes (2 floats), we must update arm angle and translation
                {
                    for (int j = 1; j < 9; j += 4) //stores two values in temp at indices 1 and 5
                        temp[j] = decoded[j];

                    //retrieves the two stored values
                    float angleTarget = BitConverter.ToSingle(temp, 1);
                    float translationTarget = BitConverter.ToSingle(temp, 5);

                    //find difference b/w current and target angle
                    float angleNow = (float)pot1.Read();
                    float angleDiff = angleTarget - angleNow;

                    //keep moving the motor in the correct direction until the angle difference is small enough
                    while (System.Math.Abs(angleDiff) < 10) //shouldn't this be > 10? 
                    {
                        talons[4].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(angleNow / angleTarget)) * System.Math.Sign(angleDiff)); //Math.Sign accounts for the direction, the Math.Max term sets the percent output magnitude with a minimun of 15%?
                        angleNow = (float)pot1.Read();
                        angleDiff = angleTarget - angleNow;
                    }

                    //find difference b/w current and target translation value
                    float translationNow = (float)pot2.Read();
                    float translationDiff = translationTarget - translationNow;
                    //keep moving the motor in the correct direction until the angle difference is small enough
                    while (System.Math.Abs(translationDiff) < 10)
                    {
                        talons[5].Set(ControlMode.PercentOutput, System.Math.Max(15, System.Math.Abs(translationNow / translationTarget)) * System.Math.Sign(translationDiff));
                        translationNow = (float)pot2.Read();
                        translationDiff = translationTarget - translationNow;
                    }
                }
                decoded.RemoveFront(count + 1); // remove count and data bytes
            }
            CTRE.Phoenix.Watchdog.Feed();
        }

        // get motor currents, arm angle, and arm translation and put into dataOut
        public void GetStatus()
        {
            double val;
            byte[] bytes;
            // motor currents
            for (int i = 0; i < talons.Length; i++)
                dataOut[i] = (byte)(talons[i].GetOutputCurrent() * 4);
            // arm angle
            val = pot1.Read();
            val = (maxAngle - minAngle) * val + minAngle; // convert to angle
            bytes = BitConverter.GetBytes((float)val); // convert to byte array
            for (int i = 0; i < bytes.Length; i++) // put in dataOut
                dataOut[i + talons.Length] = bytes[i];
            // arm translation
            val = pot2.Read();
            val = (maxTrans - minTrans) * val + minTrans; // convert to translation
            //bytes = BitConverter.GetBytes((float)val); // convert to byte array
            bytes = BitConverter.GetBytes((float)1.0); // dummy value bc no pot2 yet
            for (int i = 0; i < bytes.Length; i++) // put in dataOut
                dataOut[i + talons.Length + 4] = bytes[i];
        }
    }
}