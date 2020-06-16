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
        private Int32 selfAddress;
        public Frame RequestFrame { get; set; }

        public Int32 SelfAddress { get => selfAddress; set => selfAddress = value; }

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
            if (stations.Equals("MASTER"))
            {
                //Ogranicz czas wykonania transakcji
                _serialPort.ReadTimeout =portParameters.readTimeout;
                _serialPort.WriteTimeout =portParameters.readTimeout;
            }
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

        public string SendMessage(string address, string instruction, string message)
        {
            if (_serialPort.IsOpen)
            {
                if (address == "")
                    address = "0";
                string msg = BuildFrame(Int16.Parse(address), Int16.Parse(instruction), message);
                msg = ":" + msg + Convert.ToByte(CalculateLRC(msg)).ToString("x2") + "\r\n";
                _serialPort.WriteLine(msg);
                return msg;
            }

            return "";
        }

        public Tuple<string, bool> ReceiveMessage()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    // read the message and extract data from it
                    string message = _serialPort.ReadLine();
                    string frame = message;
                    byte address = Convert.ToByte(message.Substring(1, 2), 16);
                    byte instruction = Convert.ToByte(message.Substring(3, 2), 16);
                    byte LRC = Convert.ToByte(message.Substring(message.Length - 5, 2), 16);
                    string msg = "";
                    for(int i = 5; i < message.Length - 4; i += 2)
                    {
                        msg += Convert.ToChar(Convert.ToByte(message.Substring(i, 2), 16)).ToString();
                    }

                    if (Convert.ToInt32(address) == selfAddress || Convert.ToInt32(address) == 0)
                        message = msg;
                    else
                        return null;

                    if (instruction == 1)
                    { 
                            return new Tuple <string, bool> ($"[in]: {message} [hex frame]: {frame}", false);
                    }
                    if(instruction == 2)
                    {
                        if (stations.Equals("SLAVE"))
                            return new Tuple<string, bool>($"[in]: {message} [hex frame]: {frame}", true);
                        if(stations.Equals("MASTER"))
                            return new Tuple<string, bool>($"[in]: {message} [hex frame]: {frame}", false);
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
