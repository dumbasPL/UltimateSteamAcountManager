using System;
using System.Windows.Forms;

namespace Ultimate_Steam_Acount_Manager
{
    public partial class AddAcountForm : Form
    {
        public string Username;
        public string Password;
        public string SharedSecret;
        public string Comment;

        public AddAcountForm()
        {
            InitializeComponent();
        }

        public AddAcountForm(string username, string password, string sharedSecret, string comment) : this()
        {
            UsenameBox.Text = username;
            PassBox.Text = password;
            SecretBox.Text = sharedSecret;
            CommentBox.Text = comment;
            Text = "Edit account";
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            Username = UsenameBox.Text;
            Password = PassBox.Text;
            SharedSecret = SecretBox.Text;
            Comment = CommentBox.Text;
        }

        private void ShowPass_Click(object sender, EventArgs e)
        {
            PassBox.UseSystemPasswordChar = !PassBox.UseSystemPasswordChar;
            ((Button)sender).Text = PassBox.UseSystemPasswordChar ? "Show" : "Hide";
        }

        private void ShowSecret_Click(object sender, EventArgs e)
        {
            SecretBox.UseSystemPasswordChar = !SecretBox.UseSystemPasswordChar;
            ((Button)sender).Text = SecretBox.UseSystemPasswordChar ? "Show" : "Hide";

        }
    }
}
