using System;
using CTRE.Phoenix;

namespace HERO_Serial
{
    class Utils
    {
        public static byte CurrentEncode(float current)
        {
            return (byte)(current * 4); // equivalent to << 2
        }
        public static byte[] CurrentEncodeArray(float[] current)
        {
            byte[] output = new byte[current.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = CurrentEncode(current[i]);
            }
            return output; 
        }
        public static float thresh(float a, float th)
        {

            return Util.Abs(a) < Util.Abs(th) ? a : Util.Abs(th) * System.Math.Sign(a);
        }
        public static string ArrToString(float[] arr)
        {
            string val = "";
            for (int i = 0; i < arr.Length; i++)
                val += arr[i] + ", ";
            return val;
        }
        public static byte[] MakeByteArrayFromString(String msg)
        {
            byte[] retval = new byte[msg.Length];
            for (int i = 0; i < msg.Length; ++i)
                retval[i] = (byte)msg[i];
            return retval;
        }
        public static int sum(byte[] data, int start, int end)
        {
            int s = 0;
            for (int i = start; i < end; i++)
                s += data[i];
            return s;
        }
    }
}
