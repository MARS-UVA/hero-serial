using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using HERO_Serial;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.Controller;
using Microsoft.SPOT;
using System;
using System.Threading;

namespace HERO_Serial
{
    class IRSensors
    {
        readonly AnalogInput pot0 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);

        public IRSensors()
        {

        }

        public double get_IR_readings()
        {
            double read0 = pot0.Read();
            return read0;
        }
    }
}