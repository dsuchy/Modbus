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

        public void SendMessage(string message)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.WriteLine(message);
            }
        }

        public Tuple<string, bool> ReceiveMessage()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    string message = _serialPort.ReadLine();

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
