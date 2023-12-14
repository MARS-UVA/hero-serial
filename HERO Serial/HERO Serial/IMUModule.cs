using CTRE.Phoenix;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;


public class IMUModule
{
    private static I2CDevice MyI2C = null;
    private static I2CDevice.I2CTransaction[] ReadCommand;
    static int ReadCheck = 0;


    public IMUModule(byte DeviceAddress, int ClockRate)
	{
        I2CDevice.Configuration SonarConfig = new I2CDevice.Configuration(DeviceAddress, ClockRate);
        MyI2C = new I2CDevice(SonarConfig);
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

        // gyro data stored in address 0x1D-0x22 (decimal)
        byte XHighAdd = 0x1D;
        byte XLowAdd = 0x1E;
        byte YHighAdd = 0x1F;
        byte YLowAdd = 0x20; 
        byte ZHighAdd = 0x21;
        byte ZLowAdd = 0x22;
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
}
