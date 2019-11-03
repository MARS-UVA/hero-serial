using System;
using Microsoft.SPOT;

namespace HERO_Serial
{
    class Utils
    {
        public static float abs(float a)
        {
            return a < 0 ? -a : a;
        }
        public static float thresh(float a, float th)
        {
            return abs(a) < th ? 0 : a;
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
    }
}
