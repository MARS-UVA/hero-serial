using CTRE.Phoenix;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;


public class IMUModule
{
    private static I2CDevice MyI2C = null;
    private static I2CDevice.I2CTransaction[] ReadCommand;
    private static I2CDevice.I2CTransaction[] WriteCommand;
    static int ReadCheck = 0;


    public IMUModule(byte DeviceAddress, int ClockRate)
	{
        I2CDevice.Configuration SonarConfig = new I2CDevice.Configuration(DeviceAddress, ClockRate);
        MyI2C = new I2CDevice(SonarConfig);
    }

    //I2C write function that takes the address and data
    private static void I2CWrite(byte Address, byte Data)
    {
        WriteCommand = new I2CDevice.I2CTransaction[1];
        WriteCommand[0] = I2CDevice.CreateWriteTransaction(new byte[2]);
        WriteCommand[0].Buffer[0] = Address;
        WriteCommand[0].Buffer[1] = Data;
        MyI2C.Execute(WriteCommand, 100);
    }

    //I2C read function that takes the address, and buffer, and returns amount of transactions
    private static int I2CRead(byte Address, byte[] Data)
    {
        ReadCommand = new I2CDevice.I2CTransaction[2];
        ReadCommand[0] = I2CDevice.CreateWriteTransaction(new byte[] { Address });
        ReadCommand[1] = I2CDevice.CreateReadTransaction(Data);
        ReadCheck = MyI2C.Execute(ReadCommand, 100);
        return ReadCheck;
    }

    // Change Adafruit IMU Operation Mode (default: CONFIGMODE)
    public void changeOpMode(byte Mode)
    {
        byte opModeAddress = 0x3D; // 0x3D = 0b00111101
        IMUModule.I2CWrite(opModeAddress, Mode);
    }

    // to verify device address
    public byte readAddress()
    {
        byte[] deviceAddress = new byte[1];
        int readcheck = I2CRead(0, deviceAddress);
        Debug.Print("Read check: " + readcheck.ToString());
        return deviceAddress[0];
    }

    // returns the gyroscope data (angular velocities in x,y,z directions) from the IMU
    public uint[] ReadGyroscopeData()
    {
        uint[] gyroData = new uint[3];

        // gyro data stored in address 0x14-0x18 (decimal)
        byte XHighAdd = 0x15;
        byte XLowAdd = 0x14;
        byte YHighAdd = 0x17;
        byte YLowAdd = 0x16; 
        byte ZHighAdd = 0x19;
        byte ZLowAdd = 0x18;
        // define buffers
        byte[] XHigh = new byte[1];
        Debug.Print("XHigh" + XHigh[0].ToString());
        byte[] XLow = new byte[1];
        byte[] YHigh = new byte[1];
        byte[] YLow = new byte[1]; 
        byte[] ZHigh = new byte[1];
        byte[] ZLow = new byte[1];
        // address argument could be the register address on the IMU
        // includes x,y,z axes, each has high and low (since the data is 16 bits, or 2 bytes)

        Thread.Sleep(30);

        // read data into buffers
        I2CRead(XHighAdd, XHigh);
        I2CRead(XLowAdd, XLow);
        I2CRead(YHighAdd, YHigh);
        I2CRead(YLowAdd, YLow);
        I2CRead(ZHighAdd, ZHigh);
        I2CRead(ZLowAdd, ZLow);

        // combine high and low bytes
        gyroData[0] = XHigh[0];
        gyroData[0] <<= 8;
        gyroData[0] |= XLow[0];
        gyroData[1] = YHigh[0];
        gyroData[1] <<= 8;
        gyroData[1] |= YLow[0];
        gyroData[2] = ZHigh[0];
        gyroData[2] <<= 8;
        gyroData[2] |= ZLow[0];

        return gyroData;
    }

    public uint[] ReadAccelerometerData()
    {
        uint[] accelData = new uint[3];

        // accel data stored in address 0x08-0x0D (decimal)
        
        byte XHighAdd = 0x09;
        byte XLowAdd = 0x08;
        byte YHighAdd = 0x0B;
        byte YLowAdd = 0x0A;
        byte ZHighAdd = 0x0D;
        byte ZLowAdd = 0x0C;
        
        /*
        byte XHighAdd = 0x15;
        byte XLowAdd = 0x14;
        byte YHighAdd = 0x17;
        byte YLowAdd = 0x16;
        byte ZHighAdd = 0x19;
        byte ZLowAdd = 0x18;
        */
        // define buffers
        byte[] XHigh = new byte[1];
        Debug.Print("XHigh" + XHigh[0].ToString());
        byte[] XLow = new byte[1];
        byte[] YHigh = new byte[1];
        byte[] YLow = new byte[1];
        byte[] ZHigh = new byte[1];
        byte[] ZLow = new byte[1];
        // address argument could be the register address on the IMU
        // includes x,y,z axes, each has high and low (since the data is 16 bits, or 2 bytes)

        Thread.Sleep(30);

        // read data into buffers
        I2CRead(XHighAdd, XHigh);
        I2CRead(XLowAdd, XLow);
        I2CRead(YHighAdd, YHigh);
        I2CRead(YLowAdd, YLow);
        I2CRead(ZHighAdd, ZHigh);
        I2CRead(ZLowAdd, ZLow);

        // combine high and low bytes
        accelData[0] = XHigh[0];
        accelData[0] <<= 8;
        accelData[0] |= XLow[0];
        accelData[1] = YHigh[0];
        accelData[1] <<= 8;
        accelData[1] |= YLow[0];
        accelData[2] = ZHigh[0];
        accelData[2] <<= 8;
        accelData[2] |= ZLow[0];

        return accelData;
    }
}
