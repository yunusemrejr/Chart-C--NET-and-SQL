using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com");
            }
            catch
            {
                MessageBox.Show("Unable to open link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
