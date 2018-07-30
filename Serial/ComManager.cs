﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    public sealed class ComManager
    {
        private ComManager()
        {
            port = new SerialPort();
        }

        public static ComManager Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ComManager instance = new ComManager();
        }

        private SerialPort port;

        public void OpenPort(
            string portName,
            int baudRate,
            int dataBits = 8,
            Handshake handshake = Handshake.None,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One)
        {
            ClosePort();
            port.PortName = portName;
            port.BaudRate = baudRate;
            port.DataReceived += DataIncoming;
            try
            {
                port.Open();
            }
            catch
            {

            }
        }

        public void ClosePort()
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }

        public string ReceiveData()
        {
            return port.ReadExisting();
        }

        public event SerialDataReceivedEventHandler DataIncoming;
    }
}