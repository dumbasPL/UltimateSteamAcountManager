using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Ultimate_Steam_Acount_Manager
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            TitleLbl.Text = Application.ProductName;
            byLbl.Text = "v" + Application.ProductVersion + " - by dumbasPL";
        }

    }
}
