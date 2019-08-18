namespace Ultimate_Steam_Acount_Manager
{
    partial class AddAcountForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddAcountForm));
            this.label1 = new System.Windows.Forms.Label();
            this.UsenameBox = new System.Windows.Forms.TextBox();
            this.PassBox = new System.Windows.Forms.TextBox();
            this.SecretBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CommentBox = new System.Windows.Forms.TextBox();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.AddBtn = new System.Windows.Forms.Button();
            this.ShowSecret = new System.Windows.Forms.Button();
            this.ShowPass = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Usename:";
            // 
            // UsenameBox
            // 
            this.UsenameBox.Location = new System.Drawing.Point(165, 17);
            this.UsenameBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.UsenameBox.Name = "UsenameBox";
            this.UsenameBox.Size = new System.Drawing.Size(356, 26);
            this.UsenameBox.TabIndex = 1;
            // 
            // PassBox
            // 
            this.PassBox.Location = new System.Drawing.Point(165, 59);
            this.PassBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PassBox.Name = "PassBox";
            this.PassBox.Size = new System.Drawing.Size(292, 26);
            this.PassBox.TabIndex = 2;
            this.PassBox.UseSystemPasswordChar = true;
            // 
            // SecretBox
            // 
            this.SecretBox.Location = new System.Drawing.Point(165, 99);
            this.SecretBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SecretBox.Name = "SecretBox";
            this.SecretBox.Size = new System.Drawing.Size(292, 26);
            this.SecretBox.TabIndex = 3;
            this.SecretBox.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 62);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Password:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 102);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(144, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "2FA shared secret:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 135);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Comment:";
            // 
            // CommentBox
            // 
            this.CommentBox.AcceptsReturn = true;
            this.CommentBox.Location = new System.Drawing.Point(165, 134);
            this.CommentBox.Multiline = true;
            this.CommentBox.Name = "CommentBox";
            this.CommentBox.Size = new System.Drawing.Size(356, 125);
            this.CommentBox.TabIndex = 7;
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(429, 265);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(93, 34);
            this.CancelBtn.TabIndex = 9;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // AddBtn
            // 
            this.AddBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.AddBtn.Location = new System.Drawing.Point(330, 265);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(93, 34);
            this.AddBtn.TabIndex = 10;
            this.AddBtn.Text = "Add";
            this.AddBtn.UseVisualStyleBackColor = true;
            this.AddBtn.Click += new System.EventHandler(this.AddBtn_Click);
            // 
            // ShowSecret
            // 
            this.ShowSecret.Location = new System.Drawing.Point(464, 99);
            this.ShowSecret.Name = "ShowSecret";
            this.ShowSecret.Size = new System.Drawing.Size(57, 26);
            this.ShowSecret.TabIndex = 12;
            this.ShowSecret.Text = "Show";
            this.ShowSecret.UseVisualStyleBackColor = true;
            this.ShowSecret.Click += new System.EventHandler(this.ShowSecret_Click);
            // 
            // ShowPass
            // 
            this.ShowPass.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ShowPass.Location = new System.Drawing.Point(464, 59);
            this.ShowPass.Name = "ShowPass";
            this.ShowPass.Size = new System.Drawing.Size(57, 26);
            this.ShowPass.TabIndex = 13;
            this.ShowPass.Text = "Show";
            this.ShowPass.UseVisualStyleBackColor = true;
            this.ShowPass.Click += new System.EventHandler(this.ShowPass_Click);
            // 
            // AddAcountForm
            // 
            this.AcceptButton = this.AddBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(534, 311);
            this.Controls.Add(this.ShowPass);
            this.Controls.Add(this.ShowSecret);
            this.Controls.Add(this.AddBtn);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.CommentBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SecretBox);
            this.Controls.Add(this.PassBox);
            this.Controls.Add(this.UsenameBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "AddAcountForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add account";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox UsenameBox;
        private System.Windows.Forms.TextBox PassBox;
        private System.Windows.Forms.TextBox SecretBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox CommentBox;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button AddBtn;
        private System.Windows.Forms.Button ShowSecret;
        private System.Windows.Forms.Button ShowPass;
    }
}