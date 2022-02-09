using System;
using CTRE.Phoenix;

namespace HERO_Serial
{
    class Utils
    {
        public static float thresh(float a, float th)
        {
            return Util.Abs(a) < th ? a : th;
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
