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
    public partial class EncryptionProgressForm : Form
    {
        private delegate void SafeCallDelegate(int done, int max = -1);

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams {
            get {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle |= CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        public EncryptionProgressForm(bool decrypt = false)
        {
            InitializeComponent();
            if (decrypt)
            {
                Text = "Decryption progress";
                label1.Text = "Decrypting accounts";
            }
        }

        public void SetProgress(int done, int max = -1)
        {
            if (progressBar1.InvokeRequired)
                Invoke(new SafeCallDelegate(SetProgress), new object[] { done, max });
            else
            {
                if (max >= 0) progressBar1.Maximum = max;
                progressBar1.Value = done;
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Normal);
                TaskbarProgress.SetValue(Handle, done, progressBar1.Maximum);
            }
        }

    }
}
