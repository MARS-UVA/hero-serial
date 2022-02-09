using System;
using Microsoft.SPOT;

namespace HERO_Serial
{
    class Serial
    {
        readonly System.IO.Ports.SerialPort _uart = new System.IO.Ports.SerialPort(CTRE.HERO.IO.Port1.UART, 115200);
        readonly byte[] temp = new byte[1];
        readonly RingBuffer rBuffer = new RingBuffer(1024);

        // change it to normal array?
        public readonly RingBuffer decoded = new RingBuffer(1024);

        public Serial()
        {
            _uart.Open();
            _uart.DiscardInBuffer();
            _uart.DiscardOutBuffer();
        }

        public void ReadFromSerial()
        {
            /* read bytes out of uart */
            for (int toRead = _uart.BytesToRead; toRead > 0; toRead--)
            {
                _uart.Read(temp, 0, 1);
                rBuffer.Add(temp[0]);
                Debug.Print("reading " + temp[0].ToString());       //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            while (rBuffer.size > 2)
            {
                int flag = 0;
                for (int i = 0; i < rBuffer.size - 2; i++)
                {
                    // check for header
                    // first two bits of count dont need to be 1 anymore!
                    if (rBuffer[i] == 0xff)
                    {
                        //Debug.Print("good header");                 //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        int count = rBuffer[i + 1] & 0x3F;
                        int expected_len = 3 + count; // header + count + checksum + payload_size
                        int end = expected_len + i;
                        if (rBuffer.size >= end)
                        {
                            
                            byte sum = 0;
                            for (int j = i; j < end - 1; j++) {
                                sum += rBuffer[j];
                            }

                            // verify the checksum
                            if (sum == rBuffer[end - 1])
                            {
                                //Debug.Print("good checksum");       //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                // copy to decoded
                                // strip header and checksum
                                for (int j = i + 1; j < end - 1; j++)
                                    decoded.Add(rBuffer[j]);
                            }
                            
                            // remove bytes already read
                            rBuffer.RemoveFront(end);
                            flag = 1;
                        }
                        else // not enough bytes, need to wait for incoming bytes
                        {
                            // remove bytes already read
                            rBuffer.RemoveFront(i);
                            flag = 2;
                        }
                        break;
                    }
                }
                if (flag == 0)
                    rBuffer.Clear(); // no header found: clear buffer
                if (flag == 2)
                    break;
            }
        }
        public byte[] SendBytes(byte[] data)
        {
            var encoded = new byte[data.Length + 3];
            encoded[0] = 0xff;
            encoded[1] = (byte)(data.Length | 192);
            Array.Copy(data, 0, encoded, 2, data.Length);
            encoded[encoded.Length - 1] = (byte)(Utils.sum(encoded, 0, encoded.Length - 1) & 0xFF);

            if (_uart.CanWrite)
                _uart.Write(encoded, 0, encoded.Length);

            return encoded;
        }
    }

    class RingBuffer
    {
        readonly byte[] buffer;

        int start = 0;

        public int size = 0;
        public readonly int capacity;
        public RingBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new byte[capacity];
        }
        public byte this[int i]
        {
            get { return buffer[(i + start) % capacity]; }
            set { buffer[(i + start) % capacity] = value; }
        }
        public void Add(byte val)
        {
            this[size] = val;
            // start overwriting old values if full
            if (size == capacity)
            {
                start++;
                start %= capacity;
            }
            else
            {
                size++;
            }
        }
        public void RemoveFront(int len)
        {
            start += len;
            start %= capacity;
            size -= len;
        }
        public void RemoveBack(int len)
        {
            size -= len;
        }
        public void Clear()
        {
            start = 0;
            size = 0;
        }
        public override string ToString()
        {
            string o = "";
            for (int i = 0; i < size; i++)
            {
                o += this[i] + ",";
            }
            return o;
        }
    }
}
