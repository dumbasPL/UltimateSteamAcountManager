using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace Ultimate_Steam_Acount_Manager
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void BrowseBtm_Click(object sender, EventArgs e)
        {
            string path = null;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key != null) path = key.GetValue("SteamPath").ToString();
            }
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Steam.exe|Steam.exe",
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                InitialDirectory = path,
                ValidateNames = true,
                Title = "Select Steam.exe"
            };
            if(dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
                steamPathBox.Text = dialog.FileName;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            Manifest manifest = Manifest.GetManifest();
            steamPathBox.Text = manifest.steamPath;
            steamArgsBox.Text = manifest.steamArguments;
            paramRadio.Checked = manifest.loginMethod == LoginMethod.Parameter;
            InjectRadio.Checked = manifest.loginMethod == LoginMethod.Injectrion;
            retryOnCrashBox.Checked = manifest.retryOnCrash;
            retryOnCrashBox.Enabled = InjectRadio.Checked;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            Manifest manifest = Manifest.GetManifest();
            manifest.steamPath = steamPathBox.Text;
            manifest.steamArguments = steamArgsBox.Text;
            if (paramRadio.Checked) manifest.loginMethod = LoginMethod.Parameter;
            if (InjectRadio.Checked) manifest.loginMethod = LoginMethod.Injectrion;
            if (!manifest.Save()) MessageBox.Show("Failed to save config", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void InjectRadio_CheckedChanged(object sender, EventArgs e) => retryOnCrashBox.Enabled = InjectRadio.Checked;
    }
}
