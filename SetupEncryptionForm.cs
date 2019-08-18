using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ultimate_Steam_Acount_Manager
{
    public partial class SetupEncryptionForm : Form
    {
        public string passcode;
        public SetupEncryptionForm()
        {
            InitializeComponent();
        }

        private void SetBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pass1.Text))
            {
                errorLbl.Text = "Password can't be empty!";
                return;
            }
            if(pass1.Text != pass2.Text)
            {
                errorLbl.Text = "Passwords don't match!";
                return;
            }
            passcode = pass1.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
