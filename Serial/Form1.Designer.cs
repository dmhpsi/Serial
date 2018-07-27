namespace Serial
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layout_MainContainer = new System.Windows.Forms.TableLayoutPanel();
            this.combo_ComList = new System.Windows.Forms.ComboBox();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.btn_Open = new System.Windows.Forms.Button();
            this.btn_Send = new System.Windows.Forms.Button();
            this.txt_Message = new System.Windows.Forms.TextBox();
            this.txt_Console = new System.Windows.Forms.TextBox();
            this.txt_Status = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.layout_MainContainer.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // layout_MainContainer
            // 
            this.layout_MainContainer.ColumnCount = 5;
            this.layout_MainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.layout_MainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.layout_MainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.layout_MainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout_MainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.layout_MainContainer.Controls.Add(this.combo_ComList, 0, 1);
            this.layout_MainContainer.Controls.Add(this.btn_Refresh, 1, 1);
            this.layout_MainContainer.Controls.Add(this.btn_Open, 2, 1);
            this.layout_MainContainer.Controls.Add(this.btn_Send, 4, 1);
            this.layout_MainContainer.Controls.Add(this.txt_Message, 3, 1);
            this.layout_MainContainer.Controls.Add(this.txt_Status, 0, 2);
            this.layout_MainContainer.Controls.Add(this.tabControl1, 0, 0);
            this.layout_MainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout_MainContainer.Location = new System.Drawing.Point(0, 0);
            this.layout_MainContainer.Name = "layout_MainContainer";
            this.layout_MainContainer.RowCount = 3;
            this.layout_MainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout_MainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout_MainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout_MainContainer.Size = new System.Drawing.Size(935, 490);
            this.layout_MainContainer.TabIndex = 0;
            // 
            // combo_ComList
            // 
            this.combo_ComList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo_ComList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.combo_ComList.FormattingEnabled = true;
            this.combo_ComList.ItemHeight = 25;
            this.combo_ComList.Location = new System.Drawing.Point(3, 418);
            this.combo_ComList.Name = "combo_ComList";
            this.combo_ComList.Size = new System.Drawing.Size(94, 33);
            this.combo_ComList.TabIndex = 1;
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.BackgroundImage = global::Serial.Properties.Resources.refresh;
            this.btn_Refresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Refresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Refresh.FlatAppearance.BorderSize = 0;
            this.btn_Refresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Refresh.Location = new System.Drawing.Point(103, 418);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(36, 33);
            this.btn_Refresh.TabIndex = 2;
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.RefreshClick);
            // 
            // btn_Open
            // 
            this.btn_Open.BackgroundImage = global::Serial.Properties.Resources.play;
            this.btn_Open.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Open.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Open.FlatAppearance.BorderSize = 0;
            this.btn_Open.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Open.Location = new System.Drawing.Point(145, 418);
            this.btn_Open.Name = "btn_Open";
            this.btn_Open.Size = new System.Drawing.Size(36, 33);
            this.btn_Open.TabIndex = 3;
            this.btn_Open.UseVisualStyleBackColor = true;
            this.btn_Open.Click += new System.EventHandler(this.Open_Click);
            // 
            // btn_Send
            // 
            this.btn_Send.BackgroundImage = global::Serial.Properties.Resources.send;
            this.btn_Send.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btn_Send.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Send.FlatAppearance.BorderSize = 0;
            this.btn_Send.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Send.Location = new System.Drawing.Point(896, 418);
            this.btn_Send.Name = "btn_Send";
            this.btn_Send.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btn_Send.Size = new System.Drawing.Size(36, 33);
            this.btn_Send.TabIndex = 4;
            this.btn_Send.UseVisualStyleBackColor = true;
            // 
            // txt_Message
            // 
            this.txt_Message.Dock = System.Windows.Forms.DockStyle.Top;
            this.txt_Message.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Message.Location = new System.Drawing.Point(187, 418);
            this.txt_Message.Name = "txt_Message";
            this.txt_Message.Size = new System.Drawing.Size(703, 30);
            this.txt_Message.TabIndex = 5;
            // 
            // txt_Console
            // 
            this.txt_Console.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Console.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Console.HideSelection = false;
            this.txt_Console.Location = new System.Drawing.Point(3, 3);
            this.txt_Console.MaxLength = 65536;
            this.txt_Console.Multiline = true;
            this.txt_Console.Name = "txt_Console";
            this.txt_Console.ReadOnly = true;
            this.txt_Console.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_Console.Size = new System.Drawing.Size(915, 374);
            this.txt_Console.TabIndex = 6;
            this.txt_Console.WordWrap = false;
            // 
            // txt_Status
            // 
            this.txt_Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.layout_MainContainer.SetColumnSpan(this.txt_Status, 5);
            this.txt_Status.Cursor = System.Windows.Forms.Cursors.Default;
            this.txt_Status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Status.Enabled = false;
            this.txt_Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Status.Location = new System.Drawing.Point(3, 457);
            this.txt_Status.Name = "txt_Status";
            this.txt_Status.ReadOnly = true;
            this.txt_Status.Size = new System.Drawing.Size(929, 30);
            this.txt_Status.TabIndex = 7;
            this.txt_Status.WordWrap = false;
            // 
            // tabControl1
            // 
            this.layout_MainContainer.SetColumnSpan(this.tabControl1, 5);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(929, 409);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txt_Console);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(921, 380);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(921, 380);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.comboBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(915, 374);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(3, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 24);
            this.comboBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 490);
            this.Controls.Add(this.layout_MainContainer);
            this.MinimumSize = new System.Drawing.Size(840, 480);
            this.Name = "Form1";
            this.Text = "Form1";
            this.layout_MainContainer.ResumeLayout(false);
            this.layout_MainContainer.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout_MainContainer;
        private System.Windows.Forms.ComboBox combo_ComList;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Button btn_Open;
        private System.Windows.Forms.Button btn_Send;
        private System.Windows.Forms.TextBox txt_Message;
        private System.Windows.Forms.TextBox txt_Console;
        private System.Windows.Forms.TextBox txt_Status;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

