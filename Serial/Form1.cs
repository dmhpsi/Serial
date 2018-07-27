using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace Serial
{
    public partial class Form1 : Form
    {
        bool isOpen;
        string connectedCom;
        int connectedBaudRate;

        public Form1()
        {
            InitializeComponent();
            this.btn_Open.Enabled = false;
            this.isOpen = false;
            ComSet(false);
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            this.btn_Open.Enabled = false;
            string [] ports = SerialPort.GetPortNames();
            this.combo_ComList.Items.Clear();
            this.combo_ComList.Items.AddRange(ports);
            this.combo_ComList.Refresh();
            if (this.combo_ComList.Items.Count > 0)
            {
                this.combo_ComList.SelectedIndex = 0;
                this.btn_Open.Enabled = true;
            }
            ComSet(false);
        }

        delegate void AppendTextCallback(string text);

        private void AppendText(string text)
        {
            if (this.txt_Console.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(AppendText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                int st = this.txt_Console.SelectionStart;
                int ln = this.txt_Console.SelectionLength;
                this.txt_Console.AppendText(text);
                this.txt_Console.SelectionStart = st;
                this.txt_Console.SelectionLength = ln;
            }
        }

        private void WriteToBoard(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedText = ComManager.Instance.ReceiveData();
            AppendText(receivedText.ToString());
        }

        private void ComSet(bool open, string portName = "", int baudRate = 0)
        {
            if (open)
            {
                ComManager.Instance.DataIncoming += WriteToBoard;
                ComManager.Instance.OpenPort(portName: portName, baudRate: baudRate);
                this.btn_Open.BackgroundImage = Properties.Resources.exit;
                this.connectedCom = portName;
                this.connectedBaudRate = baudRate;
            }
            else
            {
                ComManager.Instance.DataIncoming -= WriteToBoard;
                ComManager.Instance.ClosePort();
                this.btn_Open.BackgroundImage = Properties.Resources.play;
                this.connectedCom = "";
                this.connectedBaudRate = 0;
            }
            this.isOpen = open;
            txt_Status.Text = String.Format("Port: {0}     Baud Rate: {1}", this.connectedCom, this.connectedBaudRate);
        }

        private void Open_Click(object sender, EventArgs e)
        {
            ComSet(!this.isOpen, this.combo_ComList.SelectedItem.ToString(), 9600);
        }
    }
}
