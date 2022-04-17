
using System.Collections;

/**
* Has a all the constants for the robot
* Things like CAN ID's, limits, etc
*/
public static class Constants
{
    // All static
    public enum CANID : int
    {
        DRIVETRAIN_FRONT_LEFT_TALON_ID = 0,
        DRIVETRAIN_FRONT_RIGHT_TALON_ID = 1,
        DRIVETRAIN_BACK_LEFT_TALON_ID = 2,
        DRIVETRAIN_BACK_RIGHT_TALON_ID = 3,
        BUCKETLADDER_LIFTER0_TALON_ID = 15,
        BUCKETLADDER_LIFTER1_TALON_ID = 14,
        BUCKETLADDER_EXTENDER_TALON_ID = 13,
        BUCKETLADDER_CHAIN_DRIVER_TALON_ID = 12,
        DEPOSITSYSTEM_BASKET_LIFTER0_TALON_ID = 11,
        DEPOSITSYSTEM_BASKET_LIFTER1_TALON_ID = 10,
        DEPOSITSYSTEM_CONVEYOR_DRIVER_TALON_ID = 4,
        PDP_ID = 62
    }


    public class CANIterator : System.Collections.IEnumerator
    {
        private int _pos = -1;
        // Enum.GetValues() doesn't exist in this version
        // so we have to do this
        private static CANID[] CANIDArray = {
            CANID.DRIVETRAIN_FRONT_LEFT_TALON_ID,
            CANID.DRIVETRAIN_FRONT_RIGHT_TALON_ID,
            CANID.DRIVETRAIN_BACK_LEFT_TALON_ID,
            CANID.DRIVETRAIN_BACK_RIGHT_TALON_ID,
            CANID.BUCKETLADDER_LIFTER0_TALON_ID,
            CANID.BUCKETLADDER_LIFTER1_TALON_ID,
            CANID.BUCKETLADDER_EXTENDER_TALON_ID,
            CANID.BUCKETLADDER_CHAIN_DRIVER_TALON_ID,
            CANID.DEPOSITSYSTEM_BASKET_LIFTER0_TALON_ID,
            CANID.DEPOSITSYSTEM_BASKET_LIFTER1_TALON_ID,
            CANID.DEPOSITSYSTEM_CONVEYOR_DRIVER_TALON_ID
        };


        public CANIterator()
        {
            // Sort the array
            // Since it's short, this doesn't have to be fancy
            // insertion sort
            for (int i = 1; i < CANIDArray.Length; i++)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    // If the previous value is less than the test, swap them
                    if (CANIDArray[j] > CANIDArray[i])
                    {
                        CANID temp = CANIDArray[j];
                        CANIDArray[j] = CANIDArray[i];
                        CANIDArray[i] = temp;
                    }
                }
            }
        }

        public object Current  
        {
            get 
            {
                try // recommended from the C# docs
                {
                    return CANIDArray[_pos];
                }
                catch (System.IndexOutOfRangeException)
                {
                    throw new System.InvalidOperationException();
                }
                
            }
        }

        public bool MoveNext()
        {
            _pos++;
            return _pos < CANIDArray.Length;
        }

        public void Reset()
        {
            _pos = -1;
        }
    }

}

