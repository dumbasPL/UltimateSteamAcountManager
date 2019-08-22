using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteamAuth;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace Ultimate_Steam_Acount_Manager
{
    public partial class MainWindow : Form
    {
        private delegate void SafeCallDelegate(List<SteamAccount> accounts);
        private DataGridViewCellStyle curentAcoutStyle;
        private string password;
        private SteamAccount clickedAcc;
        private List<SteamAccount> accounts;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            notifyIcon1.Text = Application.ProductName;
            curentAcoutStyle = new DataGridViewCellStyle(dataGridView1.DefaultCellStyle)
            {
                BackColor = Color.LightGreen,
                SelectionBackColor = Color.Green
            };
            password = null;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => Application.Exit();

        private void Button1_Click_1(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
            {
                if (key != null)
                {
                    Process p = Process.Start(new ProcessStartInfo()
                    {
                        FileName = key.GetValue("SteamExe").ToString(),
                        WorkingDirectory = key.GetValue("SteamPath").ToString(),
                    });
                    Injection.Inject(p, "..\\..\\Release\\UsamDLL.dll", clickedAcc.Login, clickedAcc.Password, clickedAcc.Get2FACode());
                }
            }
        }

        private void AddAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddAcountForm addAcountForm = new AddAcountForm();
            DialogResult res = addAcountForm.ShowDialog();
            if(res == DialogResult.OK)
            {
                if(Util.GetAccountByName(accounts, addAcountForm.Username) != null)
                {
                    MessageBox.Show("Account with this name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                SteamAccount steamAccount = new SteamAccount(
                    addAcountForm.Username,
                    addAcountForm.Password,
                    addAcountForm.SharedSecret,
                    addAcountForm.Comment);
                if (!Manifest.GetManifest().AddAccount(steamAccount, password))
                {
                    MessageBox.Show("Failed to add account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                RefreshAccounts();
                if(Manifest.GetManifest().accounts.Count == 1 && !Manifest.GetManifest().encrypted) //only ask first time
                {
                    if (MessageBox.Show(
                        "Do you want to set up encryption to safely store account details?",
                        "Setup Encryption?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                        SetupEncryption();
                }
            }
        }

        private void RefreshAccounts()
        {
            dataGridView1.Rows.Clear();
            EncryptionProgressForm progressForm = null;
            if (!string.IsNullOrEmpty(password))
            {
                progressForm = new EncryptionProgressForm(true);
                progressForm.Show(this);
                Enabled = false;
            }
            new Thread(() => {
                accounts = Manifest.GetManifest().GetAllAccounts(password, -1, progressForm);
                if (progressForm != null)
                    Invoke(new Action(() => {
                        progressForm.Close();
                        progressForm.Dispose();
                        Enabled = true;
                        BringToFront();
                    }));
                if (accounts == null) return;
                RenderAccounts(accounts);
            }).Start();
        }

        private void RenderAccounts(List<SteamAccount> accounts)
        {
            if (dataGridView1.InvokeRequired)
                Invoke(new SafeCallDelegate(RenderAccounts), new object[] { accounts });
            else
            {
                foreach (var acc in accounts)
                {
                    DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                    row.Cells["name"].Value = acc.Login;
                    row.Cells["profileName"].Value = acc.LastName;
                    row.Cells["lastLogin"].Value = Util.GetRelativeTimeString(acc.LastLogin);
                    row.Cells["comment"].Value = acc.Comment;
                }
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            bool encrypted = Manifest.GetManifest().encrypted;
            UpdatePasswordBar(encrypted);
            if (encrypted) PromptForPassword();
            RefreshAccounts();
        }

        private void UpdatePasswordBar(bool encrypted)
        {
            Invoke(new Action(() => {
                setupEncryptionToolStripMenuItem.Visible = !encrypted;
                changePasswordToolStripMenuItem.Visible = encrypted;
                removeEncryptionToolStripMenuItem.Visible = encrypted;
            }));
        }

        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                try
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    // Can leave these here - doesn't hurt
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                    dataGridView1.Focus();
                }
                catch (Exception) { }
        }

        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string name = (string)dataGridView1.Rows[e.RowIndex].Cells["name"].Value;
            clickedAcc = Util.GetAccountByName(accounts, name);
            if (clickedAcc == null) return;
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.RowIndex != -1)
            {
                copyMobileAuthenticatorCodeToolStripMenuItem.Visible = !string.IsNullOrWhiteSpace(clickedAcc.SharedSecret);
                contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Manifest.GetManifest().DeleteAccount(clickedAcc.ManifestEntry))
                MessageBox.Show("Failed to delete account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshAccounts();
            UpdatePasswordBar(Manifest.GetManifest().encrypted);
            if (!Manifest.GetManifest().encrypted) password = null;
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddAcountForm addAcountForm = new AddAcountForm(
                clickedAcc.Login,
                clickedAcc.Password,
                clickedAcc.SharedSecret,
                clickedAcc.Comment);
            DialogResult res = addAcountForm.ShowDialog();
            if (res == DialogResult.OK)
            {
                clickedAcc.Login = addAcountForm.Username;
                clickedAcc.Password = addAcountForm.Password;
                clickedAcc.SharedSecret = addAcountForm.SharedSecret;
                clickedAcc.Comment = addAcountForm.Comment;
                if (!Manifest.GetManifest().UpdateAccount(clickedAcc.ManifestEntry, clickedAcc, password))
                {
                    MessageBox.Show("Failed to edit account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                RefreshAccounts();
            }
        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e) => RefreshAccounts();

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) => new AboutForm().ShowDialog();

        private void ExitToolStripMenuItem1_Click(object sender, EventArgs e) => Application.Exit();

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
#if DEBUG
                Application.Exit();
#else
                Hide();
#endif
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !DEBUG
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            Hide();
#endif
        }

        private void SetupEncryptionToolStripMenuItem_Click(object sender, EventArgs e) => SetupEncryption();

        private void SetupEncryption()
        {
            if (Manifest.GetManifest().encrypted) return;
            if(accounts.Count == 0)
            {
                MessageBox.Show("Add an account first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SetupEncryptionForm form = new SetupEncryptionForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                EncryptionProgressForm progressForm = new EncryptionProgressForm();
                progressForm.Show();
                Enabled = false;
                new Thread(() => {
                    bool r = Manifest.GetManifest().ChangeEncryptionKey(null, form.passcode, progressForm);
                    Enabled = true;
                    progressForm.Close();
                    progressForm.Dispose();
                    password = form.passcode;
                    MessageBox.Show(
                        this,
                        r ? "Encryption key set" : "Failed to set encryption key!",
                        r ? "Info" : "Error",
                        MessageBoxButtons.OK,
                        r ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    UpdatePasswordBar(Manifest.GetManifest().encrypted);
                }).Start();
            }
        }

        private bool PromptForPassword(bool check = false)
        {
            PasswordPromptForm promptForm = new PasswordPromptForm();
            DialogResult res;
            do
            {
                res = promptForm.ShowDialog();
                if (res != DialogResult.OK) break;
                var acc = Manifest.GetManifest().GetAllAccounts(promptForm.password, 1);
                if (acc != null)
                {
                    password = promptForm.password;
                    return true;
                }
                promptForm.Dispose();
                promptForm = new PasswordPromptForm(true);
            } while (res == DialogResult.OK);
            if (!check) Application.Exit();
            return false;
        }

        private void ChangePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Manifest.GetManifest().encrypted) return;
            if (!PromptForPassword(true)) return;
            SetupEncryptionForm form = new SetupEncryptionForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                EncryptionProgressForm progressForm = new EncryptionProgressForm();
                progressForm.Show();
                Enabled = false;
                new Thread(() => {
                    bool r = Manifest.GetManifest().ChangeEncryptionKey(password, form.passcode, progressForm);
                    Enabled = true;
                    progressForm.Close();
                    progressForm.Dispose();
                    password = form.passcode;
                    MessageBox.Show(
                        this,
                        r ? "Encryption key changed" : "Failed to change encryption key!",
                        r ? "Info" : "Error",
                        MessageBoxButtons.OK,
                        r ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    UpdatePasswordBar(Manifest.GetManifest().encrypted);
                }).Start();
            }
        }

        private void RemoveEncryptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Manifest.GetManifest().encrypted) return;
            if (!PromptForPassword(true)) return;
            if(MessageBox.Show(
                "Are you sure you want to remove encryption?", 
                "Are you sure?", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                EncryptionProgressForm progressForm = new EncryptionProgressForm(true);
                progressForm.Show();
                Enabled = false;
                new Thread(() => {
                    bool r = Manifest.GetManifest().ChangeEncryptionKey(password, null, progressForm);
                    Enabled = true;
                    progressForm.Close();
                    progressForm.Dispose();
                    password = null;
                    MessageBox.Show(
                        this,
                        r ? "Encryption removed" : "Failed to remove encryption!",
                        r ? "Info" : "Error",
                        MessageBoxButtons.OK,
                        r ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    UpdatePasswordBar(Manifest.GetManifest().encrypted);
                }).Start();
            }
        }

        private void CopyMobileAuthenticatorCodeToolStripMenuItem_Click(object sender, EventArgs e) => 
            Clipboard.SetText(clickedAcc.Get2FACode());

        //steamui.dll + 825ADC

        //55 8B EC 53 8B 1D ?? ?? ?? ?? 56 57 8B 7D 08 8B F1 68 FF FF FF 7F 68 ?? ?? ?? ?? 57 FF D3 83 C4 0C 85 C0 75 12 8D 8E EC FE FF FF E8 ?? ?? ?? ?? 5F 5E 5B 5D C2 04 00

    }
}
