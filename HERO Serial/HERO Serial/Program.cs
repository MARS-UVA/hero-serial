﻿using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.Sensors;

namespace HERO_Serial
{
    public class Program
    {
        static readonly TalonSRX[] talons = new TalonSRX[8];
        static readonly PigeonIMU pigeon;

        static Program() {
            // 11-12: left motor
            // 13-14: right motor
            // 17: (actuator) bucket ladder angle
            // 18: (actuator) unused on the old robot
            // 16: bucket ladder
            // 15: deposit bin
            int[] talonIdx = { 11, 12, 13, 14, 17, 18, 16, 15 };
            bool[] inverted = { true, true, false, false, false, false, false, false };
            for (int i = 0; i < talonIdx.Length; i++)
            {
                var t = new TalonSRX(talonIdx[i]);
                t.SetInverted(inverted[i]);
                talons[i] = t;
            }
            pigeon = new PigeonIMU(talons[3]);
        }

        public static void Main()
        {
            //while (true)
            //{
            //    float[] vals = new float[4];
            //    pigeon.GetAccelerometerAngles(vals);
            //    Debug.Print(Utils.ArrToString(vals) + " ");
            //}
            var control = new Control(talons);
            var serial = new Serial();
            while (true)
            {
                serial.ReadFromSerial();
                control.ReadAction(serial.decoded);
                serial.SendBytes(control.GetMotorCurrent());
            }
            // control.HandleXGamepad(); // for direct control
        }
    }
}
