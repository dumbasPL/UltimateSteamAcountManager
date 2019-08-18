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
    public partial class PasswordPromptForm : Form
    {
        public string password;

        public PasswordPromptForm(bool invalid = false)
        {
            InitializeComponent();
            Text = Application.ProductName;
            if (invalid) errLbl.Text = "Invalid pasword!";
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            password = passBox.Text;
        }
    }
}
