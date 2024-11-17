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
        readonly PowerDistributionPanel pdp;
        
        public readonly byte[] dataOut; // data sent back to Jetson, used to send motor current readings

        public Control()
        {
            // 4 drivetrain currents
            // 4 bucket ladder currents
            // 2 deposit bin currents
            // 1 conveyor current
            // 2 bucket ladder angles
            // 2 limit switches
            // 13 floats + 2 bytes = 54 bytes
            dataOut = new byte[48]; // TODO: this probably needs to be something else
            pdp = new PowerDistributionPanel((int)Constants.CANID.PDP_ID);
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
                //float driveForwards = gamepad.GetRightY() * 0.5f;
                //float driveTurn = gamepad.GetRightX() * 0.5f;
                float driveForwards = (float) System.Math.Pow(gamepad.GetRightY(), 3.0f); // motor control is now a cubic function of joystick input
                float driveTurn = (float)System.Math.Pow(gamepad.GetRightX(), 3.0f);


                float[] driveTrainCurrents = drivetrain.GetCurrents(pdp);
                // Pass it to the drivetrain
                drivetrain.DirectDrive(driveForwards, driveTurn, 1.0f);
           

                // Get input for the bucket ladder
                float bucketHeight = gamepad.GetLeftY();
                float bucketExtension = gamepad.GetLeftX(); // not used right now

                // temporary code Austen added for POL. Rotates bucket ladder based on analog input values from left and right triggers
                float bucketChain = gamepad.GetRightTrigger() + (gamepad.GetLeftTrigger() *  -1.0f);

                // Pass to the bucket ladder subsystem
                bucketladder.HeightDirectControl(bucketHeight, 1.0f);
                bucketladder.ExtendDirectControl(bucketExtension, 1.0f); // not used
                bucketladder.ChainDirectControl(bucketChain, 1.0f);

                // Get input for construction bin actuator (used to be called basket)
                // Y lifts the basket, A lowers it
                // This has been tested
                var basketLift = 0f;
                if (gamepad.IsYPressed() && gamepad.IsAPressed())
                {
                    basketLift = 0f;
                }
                else if (gamepad.IsYPressed())
                {
                    basketLift = 1f;
                }
                else if (gamepad.IsAPressed())
                {
                    basketLift = -1f;
                }

                // Pass it to the Deposit subsytem
                deposit.BasketLiftDirectControl(basketLift, 0.5f);

                /*// Get input for the conveyor 
                // B moves towards deposit bin, X away
                var conveyorSpeed = 0f;
                if (gamepad.IsBPressed() && gamepad.IsXPressed())
                {
                    conveyorSpeed = 0f;
                }
                else if (gamepad.IsXPressed())
                {
                    conveyorSpeed = -1f;
                }
                else if (gamepad.IsBPressed())
                {
                    conveyorSpeed = 1f;
                }

                // Pass it to the Deposit subsytem
                deposit.FlipperDirectControl(conveyorSpeed, 0.5f);*/

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
            // No loops here. This should be run from the main loop
            if (decoded.size > 0)
            {
                // Get the subsystems
                var drivetrain = Drivetrain.getInstance();
                var bucketladder = BucketLadder.getInstance();
                var deposit = DepositSystem.getInstance();

                // Get important data
                int opcode = (decoded[0] & 0xC0) >> 6; // Get the first two bits
                int count = (decoded[0] & 0x3F); // Number of data bytes

                // Opcode tells which mode of operation
                if (opcode == 0) // STOP
                {
                    drivetrain.Stop();
                    bucketladder.Stop();
                    deposit.Stop();
                } else if (opcode == 1) // Direct Control
                {
                    // remove all the data. Checksum should already have been removed
                    Constants.CANIterator talonIterator = new Constants.CANIterator();
                    for (int i = 0; i < count; i++)
                    {
                        // This is a foolish way to do this, 
                        // but it maintains modularity (I think)
                        byte byteCommand = decoded[i + 1];
                        float command = ((int)byteCommand - 100) / 100.0f; // from old code. Handles negatives?
                        if (i >= 7)
                        {
                            command = byteCommand;
                        }
                        //Debug.Print("Command: " + command.ToString());
                        if (talonIterator.MoveNext()) // must be called before Current
                        {
                            /*
                             * Order is:
                             * drive front left
                             * drive front right
                             * drive back left
                             * drive back right
                             * bucket ladder lifters
                             * bucket extenders
                             * bucket chain driver
                             * deposit bin lifters
                             * basket flipper
                             */
                            float upperbound = 1.0f;
                            float drivetrainScale = 1.0f;
                            switch(i)
                            {
                                case 0:
                                    drivetrain.DirectDriveLeft(command * drivetrainScale, upperbound); // Both front and left take the same power? 
                                    Debug.Print("Front left: " + command.ToString());
                                    break;
                                case 1:
                                    drivetrain.DirectDriveRight(command * drivetrainScale, upperbound);
                                    Debug.Print("Front right: " + command.ToString());
                                    break;
                                case 2:
                                    drivetrain.DirectDriveLeft(command * drivetrainScale, upperbound);
                                    Debug.Print("Back left: " + command.ToString());
                                    break;
                                case 3:
                                    drivetrain.DirectDriveRight(command * drivetrainScale, upperbound);
                                    Debug.Print("Back right: " + command.ToString());
                                    break;
                                case 4:
                                    bucketladder.HeightDirectControl(command, upperbound);
                                    Debug.Print("BL angle: " + command.ToString());
                                    break;
                                case 5:
                                    bucketladder.ChainDirectControl(command, upperbound);
                                    Debug.Print("BL chain: " + command.ToString());
                                    break;
                                case 6:
                                    deposit.BasketLiftDirectControl(command, upperbound);
                                    Debug.Print("DB angle: " + command.ToString());
                                    break;
                                case 7: 
                                    // IR sensor servo
                                    deposit.IRServoDirectControl(command);
                                    Debug.Print("IR Servo Angle: " + command.ToString());
                                    break;
                                case 8:
                                    // Webcam sensor servo
                                    deposit.WebcamServoDirectControl(command);
                                    Debug.Print("Webcam Servo Angle: " + command.ToString());
                                    break;
                                default:
                                    // Do nothing
                                    break;
                            }

                        }
                        
                    }
                }
                else if (opcode == 2) // PID Control
                {

                }
                else if (opcode == 3) // NOP
                {

                }
                else // Should never run, but who knows
                {
                    Debug.Print("You've reached a fourth opcode. What have you done?!");
                }

                // Removes this command from the ringbuffer
                decoded.RemoveFront(count + 1); // remove count and data bytes
            }
            CTRE.Phoenix.Watchdog.Feed();
        }

        // get motor currents, arm angle, and arm translation and put into dataOut
        public void GetStatus()
        {
            /*
             * Here's how this works: 
             * 1. Each talon's current is converted from a 4 byte float by taking the 
             * last byte and left shifting it by 2. I think this would get the last 2 significant
             * bits, but I'm not sure.
             * 2. The angle and translation data is NOT encoded. Each 4 byte float is sent
             * The ROS then looks at the last 8 bytes (2 4byte floats), interprets them as a struct 
             * of 2 floats to get their values. Very clever and efficient, but not documented
             * 3. These are put into the usual encoding with an opcode of b11 (3), which is reserved
             * and sending with the same protocol as for sending motor commands. 
             */
            //double val;
            //byte[] bytes;
            // motor currents
            //for (int i = 0; i < talons.Length; i++)
            //    dataOut[i] = (byte)(talons[i].GetOutputCurrent() * 4);
            // NEW WAY:
            // 4 drivetrain currents
            // 3 bucket ladder currents
            // 7 floats = 28 bytes
            // Each motor current is a float (4 bytes)
            // the 0-15th bytes are the current readings from the wheel motors (front left, front right, back left. back right)
            Utils.EncodeFloatToByteArray(Drivetrain.getInstance().GetCurrents(pdp)).CopyTo(dataOut, 0);
            //The 16-23rd bytes are the current readings from the bucket ladder (left actuator: 16-19, chain: 20-23)
            Utils.EncodeFloatToByteArray(BucketLadder.getInstance().GetCurrents(pdp)).CopyTo(dataOut, 16);
            // 24th to 27th bytes is the current reading from the construction bin actuator
            Utils.EncodeFloatToByteArray(DepositSystem.getInstance().GetCurrents(pdp)).CopyTo(dataOut, 24);
            // 28th to 47th bytes are the average currents
            Utils.EncodeFloatToByteArray(Drivetrain.getInstance().GetAvgCurrents()).CopyTo(dataOut, 28);

            //Debug.Print("DEPOSIT BIN RAISED: " + dataOut[52] + ", BUCKET LADDER LOWERED: " + dataOut[53]);
            // Keeping this...
            // arm angle
            //val = pot1.Read();
            //val = (maxAngle - minAngle) * val + minAngle; // convert to angle
            //bytes = BitConverter.GetBytes((float)val); // convert to byte array
            //for (int i = 0; i < bytes.Length; i++) // put in dataOut
            //    dataOut[i + talons.Length] = bytes[i];
            // arm translation
            //val = pot2.Read();
            //val = (maxTrans - minTrans) * val + minTrans; // convert to translation
            //bytes = BitConverter.GetBytes((float)val); // convert to byte array
            //bytes = BitConverter.GetBytes((float)1.0); // dummy value bc no pot2 yet
            //for (int i = 0; i < bytes.Length; i++) // put in dataOut
            //    dataOut[i + talons.Length + 4] = bytes[i];
        }
    }
}