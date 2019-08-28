namespace Ultimate_Steam_Acount_Manager
{
    partial class SettingsForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.steamPathBox = new System.Windows.Forms.TextBox();
            this.browseBtm = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.steamArgsBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.paramRadio = new System.Windows.Forms.RadioButton();
            this.InjectRadio = new System.Windows.Forms.RadioButton();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.retryOnCrashBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Steam path:";
            // 
            // steamPathBox
            // 
            this.steamPathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.steamPathBox.Location = new System.Drawing.Point(114, 6);
            this.steamPathBox.Name = "steamPathBox";
            this.steamPathBox.Size = new System.Drawing.Size(327, 26);
            this.steamPathBox.TabIndex = 1;
            // 
            // browseBtm
            // 
            this.browseBtm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseBtm.Location = new System.Drawing.Point(447, 6);
            this.browseBtm.Name = "browseBtm";
            this.browseBtm.Size = new System.Drawing.Size(75, 26);
            this.browseBtm.TabIndex = 2;
            this.browseBtm.Text = "browse";
            this.browseBtm.UseVisualStyleBackColor = true;
            this.browseBtm.Click += new System.EventHandler(this.BrowseBtm_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Steam parameters:";
            // 
            // steamArgsBox
            // 
            this.steamArgsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.steamArgsBox.Location = new System.Drawing.Point(163, 38);
            this.steamArgsBox.Name = "steamArgsBox";
            this.steamArgsBox.Size = new System.Drawing.Size(359, 26);
            this.steamArgsBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Login method:";
            // 
            // paramRadio
            // 
            this.paramRadio.AutoSize = true;
            this.paramRadio.Location = new System.Drawing.Point(128, 71);
            this.paramRadio.Name = "paramRadio";
            this.paramRadio.Size = new System.Drawing.Size(101, 24);
            this.paramRadio.TabIndex = 6;
            this.paramRadio.TabStop = true;
            this.paramRadio.Text = "Parameter";
            this.toolTip1.SetToolTip(this.paramRadio, resources.GetString("paramRadio.ToolTip"));
            this.paramRadio.UseVisualStyleBackColor = true;
            // 
            // InjectRadio
            // 
            this.InjectRadio.AutoSize = true;
            this.InjectRadio.Location = new System.Drawing.Point(235, 73);
            this.InjectRadio.Name = "InjectRadio";
            this.InjectRadio.Size = new System.Drawing.Size(87, 24);
            this.InjectRadio.TabIndex = 7;
            this.InjectRadio.TabStop = true;
            this.InjectRadio.Text = "Injection";
            this.toolTip1.SetToolTip(this.InjectRadio, "Injects code into steam process to fill out the login form automatically\r\nLess st" +
        "able but a lot more secure + supports steam guard.\r\nMay break in future with som" +
        "e steam update!");
            this.InjectRadio.UseVisualStyleBackColor = true;
            this.InjectRadio.CheckedChanged += new System.EventHandler(this.InjectRadio_CheckedChanged);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(429, 135);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(93, 34);
            this.CancelBtn.TabIndex = 10;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // OkBtn
            // 
            this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Location = new System.Drawing.Point(330, 135);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(93, 34);
            this.OkBtn.TabIndex = 11;
            this.OkBtn.Text = "Save";
            this.OkBtn.UseVisualStyleBackColor = true;
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 0;
            this.toolTip1.AutoPopDelay = 32767;
            this.toolTip1.InitialDelay = 0;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 0;
            this.toolTip1.UseAnimation = false;
            this.toolTip1.UseFading = false;
            // 
            // retryOnCrashBox
            // 
            this.retryOnCrashBox.AutoSize = true;
            this.retryOnCrashBox.Location = new System.Drawing.Point(16, 106);
            this.retryOnCrashBox.Name = "retryOnCrashBox";
            this.retryOnCrashBox.Size = new System.Drawing.Size(179, 24);
            this.retryOnCrashBox.TabIndex = 12;
            this.retryOnCrashBox.Text = "Retry on steam crash";
            this.retryOnCrashBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OkBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(534, 181);
            this.Controls.Add(this.retryOnCrashBox);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.InjectRadio);
            this.Controls.Add(this.paramRadio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.steamArgsBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.browseBtm);
            this.Controls.Add(this.steamPathBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox steamPathBox;
        private System.Windows.Forms.Button browseBtm;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox steamArgsBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton paramRadio;
        private System.Windows.Forms.RadioButton InjectRadio;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox retryOnCrashBox;
    }
}