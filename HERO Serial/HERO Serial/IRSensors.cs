using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;
using HERO_Serial;
using Microsoft.SPOT.Hardware;

public class IRSensors{

    static readonly AnalogInput pot0 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);

    double read0 = pot0.Read();

    print("Sensor sees object... " + read0);

}