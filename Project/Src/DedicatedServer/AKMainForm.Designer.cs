namespace DedicatedServer
{
	partial class AKMainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AKMainForm));
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.buttonDestroy = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.listBoxUsers = new System.Windows.Forms.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxMaps = new System.Windows.Forms.ComboBox();
            this.checkBoxLoadMapAtStartup = new System.Windows.Forms.CheckBox();
            this.buttonMapLoad = new System.Windows.Forms.Button();
            this.buttonMapUnload = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.PortTextBox = new NumericTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonMapChange = new System.Windows.Forms.Button();
            this.checkBoxAllowCustomClientCommands = new System.Windows.Forms.CheckBox();
            this.SQLCon = new System.Windows.Forms.Label();
            this.textServerName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkPrivateServer = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBoxServerPassword = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.BtnAddMap = new System.Windows.Forms.Button();
            this.listMaploop = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ntbMapTime = new NumericTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxCommands = new System.Windows.Forms.TextBox();
            this.btnCommand = new System.Windows.Forms.Button();
            this.timerseconds = new System.Windows.Forms.Timer(this.components);
            this.timermilliseconds = new System.Windows.Forms.Timer(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.numericTextBox1 = new NumericTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(273, 20);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(88, 26);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonCreate
            // 
            this.buttonCreate.Location = new System.Drawing.Point(6, 20);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(88, 26);
            this.buttonCreate.TabIndex = 0;
            this.buttonCreate.Text = "Create";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // buttonDestroy
            // 
            this.buttonDestroy.Enabled = false;
            this.buttonDestroy.Location = new System.Drawing.Point(100, 20);
            this.buttonDestroy.Name = "buttonDestroy";
            this.buttonDestroy.Size = new System.Drawing.Size(88, 26);
            this.buttonDestroy.TabIndex = 1;
            this.buttonDestroy.Text = "Destroy";
            this.buttonDestroy.UseVisualStyleBackColor = true;
            this.buttonDestroy.Click += new System.EventHandler(this.buttonDestroy_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 260);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBoxLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listBoxUsers);
            this.splitContainer1.Size = new System.Drawing.Size(806, 254);
            this.splitContainer1.SplitterDistance = 427;
            this.splitContainer1.TabIndex = 0;
            // 
            // listBoxLog
            // 
            this.listBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.IntegralHeight = false;
            this.listBoxLog.Location = new System.Drawing.Point(0, 0);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(427, 254);
            this.listBoxLog.TabIndex = 0;
            // 
            // listBoxUsers
            // 
            this.listBoxUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxUsers.FormattingEnabled = true;
            this.listBoxUsers.IntegralHeight = false;
            this.listBoxUsers.Location = new System.Drawing.Point(0, 0);
            this.listBoxUsers.Name = "listBoxUsers";
            this.listBoxUsers.Size = new System.Drawing.Size(375, 254);
            this.listBoxUsers.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Map Name:";
            // 
            // comboBoxMaps
            // 
            this.comboBoxMaps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMaps.DropDownHeight = 318;
            this.comboBoxMaps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMaps.FormattingEnabled = true;
            this.comboBoxMaps.IntegralHeight = false;
            this.comboBoxMaps.Location = new System.Drawing.Point(6, 91);
            this.comboBoxMaps.Name = "comboBoxMaps";
            this.comboBoxMaps.Size = new System.Drawing.Size(401, 21);
            this.comboBoxMaps.TabIndex = 8;
            // 
            // checkBoxLoadMapAtStartup
            // 
            this.checkBoxLoadMapAtStartup.AutoSize = true;
            this.checkBoxLoadMapAtStartup.Location = new System.Drawing.Point(6, 55);
            this.checkBoxLoadMapAtStartup.Name = "checkBoxLoadMapAtStartup";
            this.checkBoxLoadMapAtStartup.Size = new System.Drawing.Size(120, 17);
            this.checkBoxLoadMapAtStartup.TabIndex = 2;
            this.checkBoxLoadMapAtStartup.Text = "Load map at startup";
            this.checkBoxLoadMapAtStartup.UseVisualStyleBackColor = true;
            this.checkBoxLoadMapAtStartup.CheckedChanged += new System.EventHandler(this.checkBoxLoadMapAtStartup_CheckedChanged);
            // 
            // buttonMapLoad
            // 
            this.buttonMapLoad.Enabled = false;
            this.buttonMapLoad.Location = new System.Drawing.Point(3, 25);
            this.buttonMapLoad.Name = "buttonMapLoad";
            this.buttonMapLoad.Size = new System.Drawing.Size(88, 26);
            this.buttonMapLoad.TabIndex = 5;
            this.buttonMapLoad.Text = "Load";
            this.buttonMapLoad.UseVisualStyleBackColor = true;
            this.buttonMapLoad.Click += new System.EventHandler(this.buttonMapLoad_Click);
            // 
            // buttonMapUnload
            // 
            this.buttonMapUnload.Enabled = false;
            this.buttonMapUnload.Location = new System.Drawing.Point(321, 19);
            this.buttonMapUnload.Name = "buttonMapUnload";
            this.buttonMapUnload.Size = new System.Drawing.Size(88, 26);
            this.buttonMapUnload.TabIndex = 7;
            this.buttonMapUnload.Text = "Unload";
            this.buttonMapUnload.UseVisualStyleBackColor = true;
            this.buttonMapUnload.Click += new System.EventHandler(this.buttonMapUnload_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Servers:";
            // 
            // PortTextBox
            // 
            this.PortTextBox.AllowSpace = false;
            this.PortTextBox.Location = new System.Drawing.Point(115, 142);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(41, 20);
            this.PortTextBox.TabIndex = 9;
            this.PortTextBox.Text = "65533";
            this.PortTextBox.TextChanged += new System.EventHandler(this.PortTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Map:";
            // 
            // buttonMapChange
            // 
            this.buttonMapChange.Enabled = false;
            this.buttonMapChange.Location = new System.Drawing.Point(100, 25);
            this.buttonMapChange.Name = "buttonMapChange";
            this.buttonMapChange.Size = new System.Drawing.Size(88, 26);
            this.buttonMapChange.TabIndex = 6;
            this.buttonMapChange.Text = "Change";
            this.buttonMapChange.UseVisualStyleBackColor = true;
            this.buttonMapChange.Click += new System.EventHandler(this.buttonMapChange_Click);
            // 
            // checkBoxAllowCustomClientCommands
            // 
            this.checkBoxAllowCustomClientCommands.AutoSize = true;
            this.checkBoxAllowCustomClientCommands.Location = new System.Drawing.Point(15, 189);
            this.checkBoxAllowCustomClientCommands.Name = "checkBoxAllowCustomClientCommands";
            this.checkBoxAllowCustomClientCommands.Size = new System.Drawing.Size(316, 17);
            this.checkBoxAllowCustomClientCommands.TabIndex = 12;
            this.checkBoxAllowCustomClientCommands.Text = "Allow custom client commands (MapLoad, CreateMapObject).";
            this.checkBoxAllowCustomClientCommands.UseVisualStyleBackColor = true;
            this.checkBoxAllowCustomClientCommands.CheckedChanged += new System.EventHandler(this.checkBoxAllowCustomClientCommands_CheckedChanged);
            // 
            // SQLCon
            // 
            this.SQLCon.AutoSize = true;
            this.SQLCon.Location = new System.Drawing.Point(58, 232);
            this.SQLCon.Name = "SQLCon";
            this.SQLCon.Size = new System.Drawing.Size(98, 13);
            this.SQLCon.TabIndex = 12;
            this.SQLCon.Text = "Server Not Created";
            // 
            // textServerName
            // 
            this.textServerName.Location = new System.Drawing.Point(97, 56);
            this.textServerName.Name = "textServerName";
            this.textServerName.Size = new System.Drawing.Size(249, 20);
            this.textServerName.TabIndex = 1;
            this.textServerName.Text = "Assault Knights Game Server 1.30.350";
            this.textServerName.TextChanged += new System.EventHandler(this.textServerName_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Server Name:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 149);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Server Port():";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // checkPrivateServer
            // 
            this.checkPrivateServer.AutoSize = true;
            this.checkPrivateServer.Location = new System.Drawing.Point(15, 212);
            this.checkPrivateServer.Name = "checkPrivateServer";
            this.checkPrivateServer.Size = new System.Drawing.Size(123, 17);
            this.checkPrivateServer.TabIndex = 15;
            this.checkPrivateServer.Text = "Make Private Server";
            this.checkPrivateServer.UseVisualStyleBackColor = true;
            this.checkPrivateServer.CheckedChanged += new System.EventHandler(this.checkPrivateServer_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.numericTextBox1);
            this.panel1.Controls.Add(this.textBoxServerPassword);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.buttonCreate);
            this.panel1.Controls.Add(this.buttonDestroy);
            this.panel1.Controls.Add(this.SQLCon);
            this.panel1.Controls.Add(this.checkPrivateServer);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.buttonClose);
            this.panel1.Controls.Add(this.checkBoxAllowCustomClientCommands);
            this.panel1.Controls.Add(this.textServerName);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.PortTextBox);
            this.panel1.Location = new System.Drawing.Point(434, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(372, 251);
            this.panel1.TabIndex = 16;
            // 
            // textBoxServerPassword
            // 
            this.textBoxServerPassword.Location = new System.Drawing.Point(112, 163);
            this.textBoxServerPassword.Name = "textBoxServerPassword";
            this.textBoxServerPassword.PasswordChar = '*';
            this.textBoxServerPassword.Size = new System.Drawing.Size(249, 20);
            this.textBoxServerPassword.TabIndex = 18;
            this.textBoxServerPassword.Text = "$$MY_PERSONAL_IFO$$";
            this.textBoxServerPassword.UseSystemPasswordChar = true;
            this.textBoxServerPassword.TextChanged += new System.EventHandler(this.textBoxServerPassword_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(44, 170);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Password():";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 232);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Status:";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnClear);
            this.panel2.Controls.Add(this.btnRemove);
            this.panel2.Controls.Add(this.BtnAddMap);
            this.panel2.Controls.Add(this.listMaploop);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.ntbMapTime);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.buttonMapLoad);
            this.panel2.Controls.Add(this.buttonMapChange);
            this.panel2.Controls.Add(this.buttonMapUnload);
            this.panel2.Controls.Add(this.checkBoxLoadMapAtStartup);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.comboBoxMaps);
            this.panel2.Location = new System.Drawing.Point(4, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(426, 251);
            this.panel2.TabIndex = 11;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(319, 113);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(88, 26);
            this.btnClear.TabIndex = 18;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(200, 113);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(88, 26);
            this.btnRemove.TabIndex = 17;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // BtnAddMap
            // 
            this.BtnAddMap.Location = new System.Drawing.Point(88, 113);
            this.BtnAddMap.Name = "BtnAddMap";
            this.BtnAddMap.Size = new System.Drawing.Size(88, 26);
            this.BtnAddMap.TabIndex = 16;
            this.BtnAddMap.Text = "Add";
            this.BtnAddMap.UseVisualStyleBackColor = true;
            this.BtnAddMap.Click += new System.EventHandler(this.BtnAddMap_Click);
            // 
            // listMaploop
            // 
            this.listMaploop.FormattingEnabled = true;
            this.listMaploop.Location = new System.Drawing.Point(7, 142);
            this.listMaploop.Name = "listMaploop";
            this.listMaploop.Size = new System.Drawing.Size(402, 95);
            this.listMaploop.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 126);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Map Loop:";
            // 
            // ntbMapTime
            // 
            this.ntbMapTime.AllowSpace = false;
            this.ntbMapTime.Location = new System.Drawing.Point(237, 68);
            this.ntbMapTime.Name = "ntbMapTime";
            this.ntbMapTime.Size = new System.Drawing.Size(81, 20);
            this.ntbMapTime.TabIndex = 12;
            this.ntbMapTime.Text = "30";
            this.ntbMapTime.TextChanged += new System.EventHandler(this.ntbMapTime_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(173, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Map Length:";
            // 
            // textBoxCommands
            // 
            this.textBoxCommands.Location = new System.Drawing.Point(4, 520);
            this.textBoxCommands.Name = "textBoxCommands";
            this.textBoxCommands.Size = new System.Drawing.Size(426, 20);
            this.textBoxCommands.TabIndex = 17;
            // 
            // btnCommand
            // 
            this.btnCommand.Location = new System.Drawing.Point(441, 520);
            this.btnCommand.Name = "btnCommand";
            this.btnCommand.Size = new System.Drawing.Size(89, 20);
            this.btnCommand.TabIndex = 18;
            this.btnCommand.Text = "Run Command";
            this.btnCommand.UseVisualStyleBackColor = true;
            this.btnCommand.Click += new System.EventHandler(this.btnCommand_Click);
            // 
            // timerseconds
            // 
            this.timerseconds.Interval = 1000;
            // 
            // timermilliseconds
            // 
            this.timermilliseconds.Interval = 1000000;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 126);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Master Server():";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // numericTextBox1
            // 
            this.numericTextBox1.AllowSpace = false;
            this.numericTextBox1.Location = new System.Drawing.Point(115, 119);
            this.numericTextBox1.Name = "numericTextBox1";
            this.numericTextBox1.Size = new System.Drawing.Size(176, 20);
            this.numericTextBox1.TabIndex = 19;
            this.numericTextBox1.Text = "Default(encrypted_bit((127.000.000.001));";
            // 
            // AKMainForm
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ClientSize = new System.Drawing.Size(814, 554);
            this.Controls.Add(this.btnCommand);
            this.Controls.Add(this.textBoxCommands);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AKMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assault Knights Dedicated Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.Button buttonCreate;
		private System.Windows.Forms.Button buttonDestroy;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox listBoxLog;
		private System.Windows.Forms.ListBox listBoxUsers;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxMaps;
		private System.Windows.Forms.CheckBox checkBoxLoadMapAtStartup;
		private System.Windows.Forms.Button buttonMapLoad;
		private System.Windows.Forms.Button buttonMapUnload;
		private System.Windows.Forms.Label label2;

        private NumericTextBox PortTextBox;
        private System.Windows.Forms.Label SQLCon;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonMapChange;
		private System.Windows.Forms.CheckBox checkBoxAllowCustomClientCommands;
        private System.Windows.Forms.TextBox textServerName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkPrivateServer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private NumericTextBox ntbMapTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxCommands;
        private System.Windows.Forms.Button btnCommand;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxServerPassword;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button BtnAddMap;
        private System.Windows.Forms.ListBox listMaploop;
        private System.Windows.Forms.Timer timerseconds;
        private System.Windows.Forms.Timer timermilliseconds;
        private System.Windows.Forms.Label label10;
        private NumericTextBox numericTextBox1;
	}
}

