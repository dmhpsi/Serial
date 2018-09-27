using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Serial
{
    public partial class SedingFromDrawer : Form
    {
        public SedingFromDrawer()
        {
            InitializeComponent();
            this.KeyDown += SendingFormKeyDown;
            TxtInput.KeyDown += TxtInputKeyDown;
            BtnSend.Font = new Font(DataManager.Instance.fontCollection.Families[0], BtnSend.Font.Size);
        }

        private void SendingFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void TxtInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnSendClick(null, null);
            }
        }

        delegate string GetTextCallback();
        private string GetText()
        {
            if (this.TxtInput.InvokeRequired)
            {
                GetTextCallback d = new GetTextCallback(GetText);
                try
                {
                    return this.Invoke(d).ToString();
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                try
                {
                    string txt = TxtInput.Text;
                    TxtInput.Text = "";
                    return txt;
                }
                catch
                {
                    return "";
                }
            }
        }
        private void BtnSendClick(object sender, EventArgs e)
        {
            string txt = GetText().Trim();
            if (txt.Length > 0)
                ComManager.Instance.WriteLine(txt.ToLower());
        }
    }

    public static class SendingForm
    {
        public static void Show()
        {
            // using construct ensures the resources are freed when form is closed
            using (var form = new SedingFromDrawer())
            {
                form.ShowDialog();
            }
        }
    }
}
