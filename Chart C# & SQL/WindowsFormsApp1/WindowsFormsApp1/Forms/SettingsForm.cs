using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.DataAccess;
using WindowsFormsApp1.Helpers;

namespace WindowsFormsApp1.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string connectionString = ConfigHelper.GetConnectionString();
                ParseConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim().ToLower();
                    string value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "host":
                        case "server":
                            txtHost.Text = value;
                            break;
                        case "port":
                            numPort.Value = int.Parse(value);
                            break;
                        case "user":
                        case "uid":
                            txtUsername.Text = value;
                            break;
                        case "password":
                        case "pwd":
                            txtPassword.Text = value;
                            break;
                        case "database":
                            txtDatabase.Text = value;
                            break;
                    }
                }
            }
        }

        private string BuildConnectionString()
        {
            return $"host={txtHost.Text};user={txtUsername.Text};password={txtPassword.Text};port={numPort.Value};database={txtDatabase.Text}";
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Testing connection...";
                lblStatus.ForeColor = Color.Orange;
                btnTest.Enabled = false;
                btnSave.Enabled = false;

                string connectionString = BuildConnectionString();
                using (var repository = new ItemRepository(connectionString))
                {
                    bool success = await repository.TestConnectionAsync();
                    
                    if (success)
                    {
                        lblStatus.Text = "✓ Connection successful!";
                        lblStatus.ForeColor = Color.Green;
                        MessageBox.Show("Database connection successful!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        lblStatus.Text = "✗ Connection failed";
                        lblStatus.ForeColor = Color.Red;
                        MessageBox.Show("Failed to connect to database. Please check your settings.", 
                            "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "✗ Connection failed";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Connection error:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest.Enabled = true;
                btnSave.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                string connectionString = BuildConnectionString();
                ConfigHelper.SaveConnectionString(connectionString);
                
                MessageBox.Show("Settings saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Host is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Database name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return false;
            }

            return true;
        }

        private void btnLoadDefaults_Click(object sender, EventArgs e)
        {
            txtHost.Text = "localhost";
            numPort.Value = 3306;
            txtUsername.Text = "root";
            txtPassword.Text = "";
            txtDatabase.Text = "demo1";
            lblStatus.Text = "Default settings loaded";
            lblStatus.ForeColor = Color.Blue;
        }
    }
}
