using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;
using System.Net;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Configuration;

namespace Serial
{
    public partial class MainForm : Form
    {
        bool isOpen;
        string connectedCom;
        int connectedBaudRate;
        private static List<Record> recordsList;
        private static System.Windows.Forms.Timer timer;
        private int page, pageSize;
        private string currBoardId, currDevId;
        private ToolTip toolTip;
        private double MinX, MaxX, MinY, MaxY, MinY2, MaxY2;
        private List<Chart> chartsList;
        private string dateTimeFormatPattern = "dd/MM/yyyy HH:mm";
        private HttpServer server;
        private ComboObject[] intervalObjects = {
            new ComboObject("1 hour", 3600),
            new ComboObject("2 hours", 7200),
            new ComboObject("6 hours", 21600),
            new ComboObject("12 hours", 43200),
            new ComboObject("24 hours", 86400),
            new ComboObject("3 days", 259200),
            new ComboObject("7 days", 604800),
            new ComboObject("30 days", 2592000),
        };
        private ComboObject selectedInterval;
        private long lastMigrate1hour = 0, lastMigrate1min = 0;
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001

        }
        internal class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        }

        public MainForm()
        {
            InitializeComponent();

            // Init HttpServer
            // https://github.com/sta/websocket-sharp
            server = new HttpServer(80, true);
                       
            server.OnGet += (sender, e) => {
                var req = e.Request;
                var res = e.Response;

                var path = req.RawUrl;
                byte[] contents;
                if (path == "/data" || path == "/data/")
                {
                    res.ContentType = "application/json";
                    Record[] records = DataManager.Instance.GetLastData();
                    List<string> ljsons = new List<string>();
                    string ret = "{\"data\":[";
                    foreach (Record record in records)
                    {
                        ljsons.Add(record.ToJSON());
                    }
                    ret += String.Join(",",ljsons) + "]}";
                    contents = Encoding.ASCII.GetBytes(ret);
                }
                else
                {
                    contents = Encoding.ASCII.GetBytes("<html><h1>404 Not found</h1></html>");
                    res.ContentType = "text/html";
                    res.ContentEncoding = Encoding.UTF8;
                }
                res.WriteContent(contents);
            };
            server.Start();
            for (int ixx = 0; ixx < 10; ixx++)
            {
                ixx += 0;
            }

            DataManager.Instance.InitCustomFont(Properties.Resources.fa_solid_900);
            SetCustomFont();
            DataManager.Instance.Connect();
            recordsList = new List<Record>();
            timer = new System.Windows.Forms.Timer
            {
                Interval = Constants.saveInterval * 1000
            };
            timer.Tick += WriteDataTask;
            this.BtnOpen.Enabled = false;
            this.isOpen = false;
            BtnLastPage.Enabled = false;
            BtnFirstPage.Enabled = false;
            BtnPreviousPage.Enabled = false;
            BtnNextPage.Enabled = false;
            toolTip = new ToolTip
            {
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip.SetToolTip(BtnFirstPage, "First Page");
            toolTip.SetToolTip(BtnGetBoardsList, "Get Boards List");
            toolTip.SetToolTip(BtnGetDevidsList, "Get Devices List");
            toolTip.SetToolTip(BtnLastPage, "Last Page");
            toolTip.SetToolTip(BtnNextPage, "Next Page");
            toolTip.SetToolTip(BtnPreviousPage, "Previous Page");
            toolTip.SetToolTip(BtnReadDb, "Read The DataBase");
            toolTip.SetToolTip(BtnRefresh, "Get COMs list");
            toolTip.SetToolTip(BtnNetInfo, "Network Infomation");
            ComSet(false);
            this.ComboIntervalList.Items.Clear();
            this.ComboIntervalList.Items.AddRange(intervalObjects);
            this.ComboIntervalList.SelectedIndex = 1;
            this.selectedInterval = (ComboObject)ComboIntervalList.SelectedItem;
            this.ComboIntervalList.SelectedIndexChanged += delegate
            {
                this.selectedInterval = (ComboObject)ComboIntervalList.SelectedItem;
                UpdateCharts();
            };

            this.chart1.ChartAreas[0].AxisX.LabelStyle.Format = dateTimeFormatPattern;
            this.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
            this.chart1.ChartAreas[0].AxisY2.LabelStyle.Format = "0.00";
            this.chart2.ChartAreas[0].AxisX.LabelStyle.Format = dateTimeFormatPattern;
            this.chart2.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
            this.chart2.ChartAreas[0].AxisY2.LabelStyle.Format = "0.00";
            this.chart3.ChartAreas[0].AxisX.LabelStyle.Format = dateTimeFormatPattern;
            this.chart3.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
            this.chart3.ChartAreas[0].AxisY2.LabelStyle.Format = "0.00";

            NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);

        }

        ~MainForm()
        {
            timer.Stop();
            server.Stop();
            NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void SetCustomFont()
        {
            PrivateFontCollection fontCollection = DataManager.Instance.fontCollection;
            BtnRefresh.Font = new Font(fontCollection.Families[0], BtnRefresh.Font.Size);
            BtnReadDb.Font = new Font(fontCollection.Families[0], BtnReadDb.Font.Size);
            BtnOpen.Font = new Font(fontCollection.Families[0], BtnOpen.Font.Size);
            BtnGetBoardsList.Font = new Font(fontCollection.Families[0], BtnGetBoardsList.Font.Size);
            BtnGetDevidsList.Font = new Font(fontCollection.Families[0], BtnGetDevidsList.Font.Size);
            BtnFirstPage.Font = new Font(fontCollection.Families[0], BtnFirstPage.Font.Size);
            BtnPreviousPage.Font = new Font(fontCollection.Families[0], BtnPreviousPage.Font.Size);
            BtnNextPage.Font = new Font(fontCollection.Families[0], BtnNextPage.Font.Size);
            BtnLastPage.Font = new Font(fontCollection.Families[0], BtnLastPage.Font.Size);
            BtnOpenSend.Font = new Font(fontCollection.Families[0], BtnOpenSend.Font.Size);
            LabelInterval.Font = new Font(fontCollection.Families[0], LabelInterval.Font.Size);
            BtnNetInfo.Font = new Font(fontCollection.Families[0], BtnNetInfo.Font.Size);
        }

        // Refresh Serial port list dropdown
        private void RefreshClick(object sender, EventArgs e)
        {
            this.BtnOpen.Enabled = false;
            string[] ports = SerialPort.GetPortNames();
            this.ComboComList.Items.Clear();
            this.ComboComList.Items.AddRange(ports);
            this.ComboComList.Refresh();
            if (this.ComboComList.Items.Count > 0)
            {
                this.ComboComList.SelectedIndex = 0;
                this.BtnOpen.Enabled = true;
            }
            ComSet(false);
        }

        private double Floor(double x, double spacing)
        {
            return (Math.Floor(x / spacing) * spacing);
        }
        private double Ceiling(double x, double spacing)
        {
            return (Math.Ceiling(x / spacing) * spacing);
        }
        private double Round(double x, double spacing)
        {
            return (Math.Round(x / spacing) * spacing);
        }

        #region Append text to console window
        delegate void AppendTextCallback(string text);
        private void AppendText(string text)
        {
            if (this.TxtConsole.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(AppendText);
                try
                {
                    this.Invoke(d, new object[] { text });
                }
                catch
                { }
            }
            else
            {
                try
                {
                    int st = this.TxtConsole.SelectionStart;
                    int ln = this.TxtConsole.SelectionLength;
                    this.TxtConsole.AppendText(DateTime.Now.ToString("[dd/MM/yy_HH:mm:ss] ") + text);
                    this.TxtConsole.SelectionStart = st;
                    this.TxtConsole.SelectionLength = ln;
                }
                catch
                {

                }
            }
        }
        #endregion

        #region Add Data To Chart
        //private void PerformAdd(Chart chart, Record record)
        //{
        //    if (chart.Series[0].Points.Count > 9)
        //    {
        //        chart.Series[0].Points.RemoveAt(0);
        //    }
        //    float temp = record.temp;
        //    chart.Series[0].Points.AddXY(DateTime.Now, temp);
        //    string[] devids = new string[] { "1", "2", "3" };
        //    FindMinMax(chart.Series[0].Points,
        //       out double minX, out double maxX, out double minY, out double maxY);
        //    chart.ChartAreas[0].AxisX.Minimum = minX;
        //    chart.ChartAreas[0].AxisX.Maximum = maxX;
        //    chart.ChartAreas[0].AxisY.Minimum = Floor(minY - 0.05, 0.1);
        //    chart.ChartAreas[0].AxisY.Maximum = Ceiling(maxY + 0.05, 0.1);

        //    if (chart.Series[1].Points.Count > 9)
        //    {
        //        chart.Series[1].Points.RemoveAt(0);
        //    }
        //    float humidity = record.humidity;
        //    chart.Series[1].Points.AddXY(DateTime.Now, humidity);
        //    FindMinMax(chart.Series[1].Points,
        //        out minX, out maxX, out minY, out maxY);
        //    chart.ChartAreas[0].AxisY2.Minimum = Floor(minY - 0.05, 0.1);
        //    chart.ChartAreas[0].AxisY2.Maximum = Ceiling(maxY + 0.05, 0.1);
        //}
        //delegate void AddChartDataCallback(Chart chart, Record record);
        //private void AddChartData(Chart chart, Record record)
        //{
        //    if (this.TxtConsole.InvokeRequired)
        //    {
        //        AddChartDataCallback d = new AddChartDataCallback(AddChartData);
        //        this.Invoke(d, new object[] { chart, record });
        //    }
        //    else
        //    {
        //        PerformAdd(chart, record);
        //    }
        //}
        #endregion

        #region Set charts min max programmatically for better display
        private void ClearMinMax()
        {
            MinX = 9999.0;
            MinY = 9999.0;
            MinY2 = 9999.0;
            MaxX = -9999.0;
            MaxY = -9999.0;
            MaxY2 = -9999.0;
            chartsList = new List<Chart>();
        }
        private void FindMinMax(DataPointCollection collection,
            out double minX, out double maxX,
            out double minY, out double maxY)
        {
            minX = 9999.0;
            maxX = -9999.0;
            minY = 9999.0;
            maxY = -9999.0;
            foreach (DataPoint data in collection)
            {
                if (minX > data.XValue) minX = data.XValue;
                if (maxX < data.XValue) maxX = data.XValue;
                if (minY > data.YValues[0]) minY = data.YValues[0];
                if (maxY < data.YValues[0]) maxY = data.YValues[0];
            }
        }
        delegate void SetMinMaxCallback();
        private void SetMinMax()
        {
            if (this.TxtConsole.InvokeRequired)
            {
                SetMinMaxCallback d = new SetMinMaxCallback(SetMinMax);
                try
                {
                    this.Invoke(d, new object[] { });
                }
                catch
                {
                }
            }
            else
            {
                if (MinX > MaxX) MinX = MaxX;
                if (MinY > MaxY) MinY = MaxY;
                if (MinY2 > MaxY2) MinY2 = MaxY2;
                foreach (Chart chart in chartsList)
                {
                    //chart.ChartAreas[0].AxisX.Minimum = MinX;
                    //chart.ChartAreas[0].AxisX.Maximum = MaxX;
                    double dY = (MaxY - MinY) / 20.0;
                    chart.ChartAreas[0].AxisY.Minimum = Floor(MinY - dY - 0.01, 0.1);
                    chart.ChartAreas[0].AxisY.Maximum = Ceiling(MaxY + dY + 0.01, 0.1);
                    dY = (MaxY2 - MinY2) / 20.0;
                    chart.ChartAreas[0].AxisY2.Minimum = Floor(MinY2 - dY - 0.01, 0.1);
                    chart.ChartAreas[0].AxisY2.Maximum = Ceiling(MaxY2 + dY + 0.01, 0.1);
                }
            }
        }
        delegate void SetMinMaxIndividualCallback();
        private void SetMinMaxIndividual()
        {
            if (this.TxtConsole.InvokeRequired)
            {
                SetMinMaxIndividualCallback d = new SetMinMaxIndividualCallback(SetMinMaxIndividual);
                try
                {
                    this.Invoke(d, new object[] { });
                }
                catch
                {
                }
            }
            else
            {
                foreach (Chart chart in chartsList)
                {
                    //chart.ChartAreas[0].AxisX.Minimum = MinX;
                    //chart.ChartAreas[0].AxisX.Maximum = MaxX;
                    FindMinMax(chart.Series[0].Points, out double minX, out double maxX, out double minY, out double maxY);
                    double dY = (maxY - minY) / 20.0;
                    chart.ChartAreas[0].AxisY.Minimum = Floor(minY - dY - 0.01, 0.1);
                    chart.ChartAreas[0].AxisY.Maximum = Ceiling(maxY + dY + 0.01, 0.1);
                    FindMinMax(chart.Series[1].Points, out minX, out maxX, out double minY2, out double maxY2);
                    dY = (maxY2 - minY2) / 20.0;
                    chart.ChartAreas[0].AxisY2.Minimum = Floor(minY2 - dY - 0.01, 0.1);
                    chart.ChartAreas[0].AxisY2.Maximum = Ceiling(maxY2 + dY + 0.01, 0.1);
                }
            }
        }
        #endregion

        delegate void SetChartDataCallback(Chart chart, Record[] records);
        private void SetChartData(Chart chart, Record[] records)
        {

            if (this.TxtConsole.InvokeRequired)
            {
                SetChartDataCallback d = new SetChartDataCallback(SetChartData);
                this.Invoke(d, new object[] { chart, records });
            }
            else
            {
                if (records.Length > 1)
                {
                    chartsList.Add(chart);
                    chart.Series[0].Points.Clear();
                    chart.Series[1].Points.Clear();
                    foreach (Record record in records)
                    {
                        long ticks = record.GetTime() * TimeSpan.TicksPerSecond;
                        DateTime dateTime = new DateTime(ticks);
                        chart.Series[0].Points.AddXY(dateTime.ToOADate(), record.temp);
                        chart.Series[1].Points.AddXY(dateTime.ToOADate(), record.humidity);
                    }
                    //FindMinMax(chart.Series[0].Points,
                    //    out double minX, out double maxX, out double minY, out double maxY);
                    ////MinX = MinX > minX ? minX : MinX;
                    ////MaxX = MaxX < maxX ? maxX : MaxX;
                    //MinY = MinY > minY ? minY : MinY;
                    //MaxY = MaxY < maxY ? maxY : MaxY;

                    //FindMinMax(chart.Series[1].Points,
                    //    out minX, out maxX, out minY, out maxY);
                    ////MinX = MinX > minX ? minX : MinX;
                    ////MaxX = MaxX < maxX ? maxX : MaxX;
                    //MinY2 = MinY2 > minY ? minY : MinY2;
                    //MaxY2 = MaxY2 < maxY ? maxY : MaxY2;
                }
            }
        }

        private void WriteToBoard(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedText = ComManager.Instance.ReceiveData();
            if (receivedText != "Fault")
            {
                AppendText(receivedText);
                if (receivedText.StartsWith("Log: "))
                {
                    int length = receivedText.Length - 5;
                    if (length > 0)
                    {
                        DataManager.Instance.Log(receivedText.Substring(5));
                    }
                }
                else if (receivedText.StartsWith("Error: "))
                {
                    DataManager.Instance.ErrorLog(receivedText);
                }
                else
                {
                    Record record = new Record();
                    if (record.Input(receivedText))
                    {
                        recordsList.Add(record);
                    }
                }
            }
        }

        // Set serial port status and change record button's icon
        private void ComSet(bool open, string portName = "", int baudRate = 0)
        {
            if (open)
            {
                ComManager.Instance.DataIncoming += WriteToBoard;
                ComManager.Instance.Disconnected += PortDisconnected;
                if (ComManager.Instance.OpenPort(portName: portName, baudRate: baudRate))
                {
                    this.BtnOpen.Text = "\uf00d";  // Time
                    this.BtnOpen.ForeColor = Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(23)))), ((int)(((byte)(28)))));
                    toolTip.SetToolTip(BtnOpen, "Stop And Close");
                    BtnOpenSend.Enabled = true;
                    this.connectedCom = portName;
                    this.connectedBaudRate = baudRate;
                    timer.Start();
                    UpdateCharts();
                    this.isOpen = true;
                }
                else
                {
                    this.isOpen = false;
                    //MessageBox.Show(this, "Comport Open Failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CenterMessage.Show("Warning", "Com Port Open Failed!");
                }
            }
            else
            {
                ComManager.Instance.DataIncoming -= WriteToBoard;
                ComManager.Instance.Disconnected -= PortDisconnected;
                ComManager.Instance.ClosePort();
                this.BtnOpen.Text = "\uf111";    // Circle
                this.BtnOpen.ForeColor = Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(66)))), ((int)(((byte)(118)))));
                toolTip.SetToolTip(BtnOpen, "Open And Start Monitoring");
                BtnOpenSend.Enabled = false;
                this.connectedCom = "";
                this.connectedBaudRate = 0;
                timer.Stop();
                WriteDataTask(null, null);
                this.isOpen = false;
            }
            LabelStatus.Text = String.Format("Port: {0} Baud Rate: {1}", this.connectedCom, this.connectedBaudRate);
        }

        private void PortDisconnected(object sender, ComManager.SerialPortDisconnected e)
        {
            ComSet(false);
        }

        private void BtnOpenClick(object sender, EventArgs e)
        {
            ComSet(!this.isOpen, this.ComboComList.SelectedItem.ToString(), 9600);
        }

        // Database view tab pagination generator
        private void Paging(string boardid, string devid, int page, int pageSize)
        {
            if (boardid != null)
            {
                currBoardId = boardid;
            }
            else
            {
                boardid = currBoardId;
            }
            if (devid != null)
            {
                currDevId = devid;
            }
            else
            {
                devid = currDevId;
            }
            int rowsCount = DataManager.Instance.GetRowsCount(boardid, devid);
            int pageCount = (rowsCount - 1) / pageSize;
            if (page == -11)
            {
                page = pageCount;
            }
            if (page >= 0 && page <= pageCount)
            {
                this.page = page;
                this.pageSize = pageSize;

                DataSet dataSet = DataManager.Instance.GetDataSource(boardid, devid, page, pageSize);
                dataGridView1.DataSource = dataSet.Tables[0].DefaultView;
                LabelPages.Text = String.Format("{0} / {1}",
                    page + 1,
                    (DataManager.Instance.GetRowsCount(boardid, devid) - 1) / pageSize + 1);
                BtnFirstPage.Enabled = (page > 0);
                BtnPreviousPage.Enabled = (page > 0);
                BtnLastPage.Enabled = (page < pageCount);
                BtnNextPage.Enabled = (page < pageCount);
            }
        }

        private void BtnReadDbClick(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = true;
            string boardid = "[All]", devid = "[All]";
            if (BoardsList.SelectedItem != null)
            {
                boardid = BoardsList.SelectedItem.ToString();
                if (DevidsList.SelectedItem != null)
                {
                    devid = DevidsList.SelectedItem.ToString();
                }
            }
            Paging(boardid, devid, 0, 10);
        }

        private void WriteDataTask(object sender, EventArgs e)
        {
            //recordsListClone = new List<Record>(recordsList.ToArray());
            //recordsList.Clear();
            //if (recordsListClone.Count <= 0)
            //{
            //    return;
            //}
            Thread thread = new Thread(DataTask);
            thread.Start();
        }

        private void BtnOpenSend_Click(object sender, EventArgs e)
        {
            SendingForm.Show();
        }

        private void LabelStatus_Click(object sender, EventArgs e)
        {
            if (LabelStatus.Text.StartsWith("P"))
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                string ips = "";
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ips += ip.ToString() + "  ";
                    }
                }
                LabelStatus.Text = "IP: " + ips;
            }
            else
            {
                LabelStatus.Text = String.Format("Port: {0} Baud Rate: {1}", this.connectedCom, this.connectedBaudRate);
            }
        }

        private void BtnNetInfo_Click(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string msg = "Status: ";
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                msg += "Conneted\nIP: ";
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        msg += ip.ToString() + "\n";
                    }
                }
            }
            else
            {
                msg += "Disconnected";
            }
            MessageBox.Show(msg, "Network Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DataTask()
        {
            if (recordsList.Count <= 0)
                return;
            Record[] records = recordsList.ToArray();
            foreach (Record record in records)
            {
                record.UnsafeSave();
            }
            recordsList.Clear();
            UpdateCharts();
            if (lastMigrate1min == 0)
                lastMigrate1min = DataManager.Instance.GetLastMigrateTimestamp(DataManager.DbSelect.avg1min);
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            if (now - lastMigrate1min > 60)
            {
                long standardTime = now / 60 * 60;
                DataManager.Instance.Migrate(DataManager.DbSelect.raw, DataManager.DbSelect.avg1min, standardTime);
                lastMigrate1min = standardTime;
            }
            if (lastMigrate1hour == 0)
                lastMigrate1hour = DataManager.Instance.GetLastMigrateTimestamp(DataManager.DbSelect.avg1hour);
            if (now - lastMigrate1hour > 3600)
            {
                long standardTime = now / 3600 * 3600;
                DataManager.Instance.Migrate(DataManager.DbSelect.avg1min, DataManager.DbSelect.avg1hour, standardTime);
                lastMigrate1hour = standardTime;
            }
        }
        private void BtnGetBoardsListClick(object sender, EventArgs e)
        {
            object selected = BoardsList.SelectedItem;
            BoardsList.Items.Clear();
            BoardsList.Items.Add("[All]");
            BoardsList.Items.AddRange(DataManager.Instance.GetBoardsList());
            BoardsList.Refresh();

            DevidsList.Items.Clear();
            DevidsList.Items.Add("[All]");
            DevidsList.Refresh();
            if (selected != null && BoardsList.Items.Contains(selected))
            {
                BoardsList.SelectedItem = selected;
            }
            else
            {
                BoardsList.SelectedIndex = 0;
                BoardsList.SelectedIndexChanged += BtnGetDevidsListClick;
            }
            BtnGetDevidsListClick(null, null);
        }
        private void BtnFirstPageClick(object sender, EventArgs e)
        {
            Paging(null, null, 0, pageSize);
        }

        private void BtnPreviousPageClick(object sender, EventArgs e)
        {
            Paging(null, null, page - 1, pageSize);
        }

        private void BtnNextPageClick(object sender, EventArgs e)
        {
            Paging(null, null, page + 1, pageSize);
        }

        private void BtnLastPageClick(object sender, EventArgs e)
        {
            Paging(null, null, -11, pageSize);
        }

        private void BtnGetDevidsListClick(object sender, EventArgs e)
        {
            if (BoardsList.SelectedItem != null)
            {
                object selected = DevidsList.SelectedItem;
                DevidsList.Items.Clear();
                DevidsList.Items.Add("[All]");
                DevidsList.Items.AddRange(DataManager.Instance.GetDevidsList(BoardsList.SelectedItem.ToString()));
                if (selected != null && DevidsList.Items.Contains(selected))
                {
                    DevidsList.SelectedItem = selected;
                }
                else
                {
                    DevidsList.SelectedIndex = 0;
                }
                DevidsList.Refresh();
            }
        }

        //private void DataTask()
        //{
        //    recordsListClone.Sort(delegate (Record x, Record y)
        //    {
        //        if (x == null && y == null) return 0;
        //        else if (x == null) return -1;
        //        else if (y == null) return 1;
        //        else return x.CompareTo(y);
        //    });
        //    Record gabbage = new Record();
        //    gabbage.Input("JUNK", "JUNK", 0.0f, 0.0f, 0);
        //    recordsListClone.Add(gabbage);
        //    Record[] records = recordsListClone.ToArray();
        //    if (records.Length > 1)
        //    {
        //        int start = 0;
        //        float mTemp = 0, mHumidity = 0;
        //        for (int i = 0; i < records.Length; i++)
        //        {
        //            if (records[start].reference != records[i].reference)
        //            {
        //                int count = i - start;
        //                mTemp /= count;
        //                mHumidity /= count;
        //                Record averageRecord = new Record();
        //                averageRecord.Input(records[start].boardid,
        //                    records[start].devid,
        //                    mTemp,
        //                    mHumidity,
        //                    count);
        //                averageRecord.SetTime(records[start].GetTime());
        //                recordsListClone.Add(averageRecord);
        //                start = i;
        //                mTemp = 0;
        //                mHumidity = 0;
        //            }
        //            mTemp += records[i].temp;
        //            mHumidity += records[i].humidity;
        //        }
        //    }
        //    else
        //    {
        //        recordsListClone.AddRange(records);
        //    }
        //    foreach (Record re in recordsListClone)
        //    {
        //        re.UnsafeSave();
        //    }
        //    UpdateCharts();
        //}

        private void UpdateCharts()
        {
            //try
            //{
            ClearMinMax();
            long historyInterval = long.Parse(selectedInterval.value.ToString());

            Record[] drawRecords = DataManager.Instance.GetDataByLastTime("mega25", "1", historyInterval);
            SetChartData(chart1, drawRecords);
            drawRecords = DataManager.Instance.GetDataByLastTime("mega25", "2", historyInterval);
            SetChartData(chart2, drawRecords);
            drawRecords = DataManager.Instance.GetDataByLastTime("mega25", "3", historyInterval);
            SetChartData(chart3, drawRecords);
            SetMinMaxIndividual();
            //}
            //catch
            //{

            //}
        }
    }
}
