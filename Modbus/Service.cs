using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string ReceiveMessage()
        {
            if (_serialPort.IsOpen)
            {
                string message = _serialPort.ReadLine();
                try
                {
                    if (!message.Contains("*&*"))
                    {
                        return $"[in] {message}";
                    }
                    else
                    {
                        message = message.Replace("*&*", "");
                        if (stations.Equals("SLAVE"))
                            SendMessage(message);
                        return $"[in] {message}";
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public Frame Execute(Frame frame)
        {
            if (IsFrameValid(frame))
            {
                if (frame.Function == Enums.Function.SEND)
                {
                    return SendRequest(frame);
                }
                else if (frame.Function == Enums.Function.GET)
                {
                    return GetRequest(frame);
                }
            }

            return null;
        }

        #region Private methods

        private bool IsFrameValid(Frame frame)
        {
            return frame.Address == RequestFrame.Address &&
                frame.Function == RequestFrame.Function;
        }

        private Frame SendRequest(Frame frame)
        {
            throw new NotImplementedException();
        }

        private Frame GetRequest(Frame frame)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
