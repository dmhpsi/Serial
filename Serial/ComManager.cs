using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    public sealed class ComManager
    {
        System.Windows.Forms.Timer timer;
        public event SerialDataReceivedEventHandler DataIncoming;
        public delegate void SerialPortDisconnected(object sender, SerialPortDisconnected e);
        public event SerialPortDisconnected Disconnected;
        private ComManager()
        {
            port = new SerialPort();
            timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            timer.Tick += CheckAlive;
        }

        private void CheckAlive(object sender, EventArgs e)
        {
            try
            {
                port.Write("\n");
            }
            catch
            {
                ClosePort();
                timer.Stop();
                Disconnected?.Invoke(null, null);
            }
        }

        public static ComManager Instance { get { return Nested.instance; } }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly ComManager instance = new ComManager();
        }

        private SerialPort port;

        public bool OpenPort(
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
            port.StopBits = StopBits.One;
            //port.NewLine = "\r\n";
            try
            {
                port.Open();
                timer.Start();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void ClosePort()
        {
            if (port.IsOpen)
            {
                timer.Stop();
                port.Close();
            }
        }
        public string ReceiveData()
        {
            string result = "";
            while (true)
            {
                char ch;
                try
                {
                    ch = (char)port.ReadChar();
                    result += ch;
                }
                catch
                {
                    return "Fault";
                }
                if (ch == '\n')
                {
                    return result;
                }
            }
        }
        public void WriteLine(string data)
        {
            try
            {
                port.WriteLine(data);
            }
            catch
            {
            }
        }
    }
}
