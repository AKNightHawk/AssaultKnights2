namespace ModPackager
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
            this.diffFiles = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.makePatch = new System.Windows.Forms.Button();
            this.inputBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.outputBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.button6 = new System.Windows.Forms.Button();
            this.firstRunPatchWorker = new System.ComponentModel.BackgroundWorker();
            this.messageText = new System.Windows.Forms.Label();
            this.patchWorker = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.backupWorker = new System.ComponentModel.BackgroundWorker();
            this.referenceFiles = new System.Windows.Forms.ComboBox();
            this.delFiles = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.txtExtensions = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // diffFiles
            // 
            this.diffFiles.FormattingEnabled = true;
            this.diffFiles.Location = new System.Drawing.Point(12, 65);
            this.diffFiles.Name = "diffFiles";
            this.diffFiles.Size = new System.Drawing.Size(466, 259);
            this.diffFiles.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Added  Files";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(484, 65);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Select All";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(484, 94);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "Deselect All";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // makePatch
            // 
            this.makePatch.Location = new System.Drawing.Point(307, 23);
            this.makePatch.Name = "makePatch";
            this.makePatch.Size = new System.Drawing.Size(75, 23);
            this.makePatch.TabIndex = 9;
            this.makePatch.Text = "Make Patch";
            this.makePatch.UseVisualStyleBackColor = true;
            this.makePatch.Click += new System.EventHandler(this.makePatch_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(226, 23);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 13;
            this.button6.Text = "Scan";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // firstRunPatchWorker
            // 
            this.firstRunPatchWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.firstRunPatchWorker_DoWork);
            this.firstRunPatchWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.firstRunPatchWorker_RunWorkerCompleted);
            // 
            // messageText
            // 
            this.messageText.Location = new System.Drawing.Point(9, 530);
            this.messageText.Name = "messageText";
            this.messageText.Size = new System.Drawing.Size(547, 16);
            this.messageText.TabIndex = 14;
            this.messageText.Text = "label1";
            // 
            // patchWorker
            // 
            this.patchWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.patchWorker_DoWork);
            this.patchWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.patchWorker_RunWorkerCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "References";
            // 
            // backupWorker
            // 
            this.backupWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backupWorker_DoWork);
            this.backupWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backupWorker_RunWorkerCompleted);
            // 
            // referenceFiles
            // 
            this.referenceFiles.FormattingEnabled = true;
            this.referenceFiles.Location = new System.Drawing.Point(12, 25);
            this.referenceFiles.Name = "referenceFiles";
            this.referenceFiles.Size = new System.Drawing.Size(208, 21);
            this.referenceFiles.TabIndex = 17;
            // 
            // delFiles
            // 
            this.delFiles.FormattingEnabled = true;
            this.delFiles.Location = new System.Drawing.Point(12, 343);
            this.delFiles.Name = "delFiles";
            this.delFiles.Size = new System.Drawing.Size(466, 184);
            this.delFiles.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 327);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Deleted  Files";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(484, 372);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 21;
            this.button1.Text = "Deselect All";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(484, 343);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 20;
            this.button4.Text = "Select All";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // txtExtensions
            // 
            this.txtExtensions.Location = new System.Drawing.Point(388, 26);
            this.txtExtensions.Name = "txtExtensions";
            this.txtExtensions.Size = new System.Drawing.Size(171, 20);
            this.txtExtensions.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(385, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Permitted Extensions";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(484, 401);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 25;
            this.button5.Text = "button5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 553);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtExtensions);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.delFiles);
            this.Controls.Add(this.referenceFiles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.messageText);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.makePatch);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.diffFiles);
            this.Name = "Form1";
            this.Text = "Mod Packager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox diffFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button makePatch;
        private System.Windows.Forms.FolderBrowserDialog inputBrowser;
        private System.Windows.Forms.FolderBrowserDialog outputBrowser;
        private System.Windows.Forms.Button button6;
        private System.ComponentModel.BackgroundWorker firstRunPatchWorker;
        private System.Windows.Forms.Label messageText;
        private System.ComponentModel.BackgroundWorker patchWorker;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker backupWorker;
        private System.Windows.Forms.ComboBox referenceFiles;
        private System.Windows.Forms.CheckedListBox delFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox txtExtensions;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button5;
    }
}

