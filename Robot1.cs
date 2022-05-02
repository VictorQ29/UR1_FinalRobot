using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountPixel2
{
    class Robot
    {
        public const byte STOP = (byte)'s';//send character 's' to stop 
        public const byte FORWARD = (byte)'f';
        public const byte LEFT = (byte)'l';
        public const byte RIGHT = (byte)'r';
        public const byte LEFT2 = (byte)'L';
        public const byte RIGHT2 = (byte)'R';
        SerialPort _serialPort;
        public bool Online { get; private set; }

        public Robot() { }

        public Robot(String port)
        {
            SetupSerialComms(port);
        }

        public void SetupSerialComms(String port) //SetupSerialComms
        {
            try
            {
                _serialPort = new SerialPort(port);
                _serialPort.BaudRate = 9600; 
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.Two;
                _serialPort.Open();
                Online = true;
            }
            catch
            {
                Online = false;
            }
        }

        public void Move(byte direction)
        {
            try
            {
                if (Online)
                {
                    byte[] buffer = { direction };
                    _serialPort.Write(buffer, 0, 1);
                }
            }
            catch
            {
                Online = false;
            }
        }

        public void Close()
        {
            _serialPort.Close();
        }

    }
}