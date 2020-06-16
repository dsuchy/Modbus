using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Modbus
{
    class Service
    {
        private readonly SerialPort _serialPort = new SerialPort();
        private string stations;
        public Frame RequestFrame { get; set; }

        private char CalculateLRC(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                LRC ^= bytes[i];
            }
            return Convert.ToChar(LRC);
        }

        private string BuildFrame(Int16 address, Int16 instruction, string message)
        {
            string msg = "";
            msg += address.ToString("x2");
            msg += instruction.ToString("x2");
            foreach (char character in message)
            {
                msg += Convert.ToByte(character).ToString("x2");
            }
            return msg;
        }

        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public bool OpenPort(PortParameters portParameters, string station)
        {
            _serialPort.BaudRate = portParameters.Speed;
            _serialPort.PortName = portParameters.PortName;
            stations = station;

            try
            {
                _serialPort.Open();
                return _serialPort.IsOpen;
            }
            catch
            {
                _serialPort.Close();
                return false;
            }
        }

        public void ClosePort()
        {
            _serialPort.Close();
        }

        public void SendMessage(string address, string instruction, string message)
        {
            if (_serialPort.IsOpen)
            {
                if (address == "")
                    address = "0";
                string msg = BuildFrame(Int16.Parse(address), Int16.Parse(instruction), message);
                msg = ":" + msg + Convert.ToByte(CalculateLRC(msg)).ToString("x2") + "\r\n";
                _serialPort.WriteLine(msg);
            }
        }

        public Tuple<string, bool> ReceiveMessage()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    // read the message and extract data from it
                    string message = _serialPort.ReadLine();
                    byte address = Convert.ToByte(message.Substring(1, 2), 16);
                    byte instruction = Convert.ToByte(message.Substring(3, 2), 16);
                    byte LRC = Convert.ToByte(message.Substring(message.Length - 5, 2), 16);
                    string msg = "";
                    for(int i = 5; i < message.Length - 4; i += 2)
                    {
                        msg += Convert.ToChar(Convert.ToByte(message.Substring(i, 2), 16)).ToString();
                    }
                    message = msg;

                    if (!message.Contains("*&*"))
                    { 
                            return new Tuple <string, bool> ($"[in] {message}", false);
                    }
                    else
                    {
                        message = message.Replace("*&*", "");
                        if (stations.Equals("SLAVE"))
                            return new Tuple<string, bool>($"[in] {message}", true);
                    }
                }
                catch (Exception e)
                {
                }
            }

            return null;
        }

    }
}
