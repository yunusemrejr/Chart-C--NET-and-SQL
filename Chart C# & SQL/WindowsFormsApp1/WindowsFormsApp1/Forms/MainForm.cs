using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.DataAccess;
using WindowsFormsApp1.Helpers;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Forms
{
    public partial class MainForm : Form
    {
        private ItemRepository repository;
        private ChartService chartService;
        private ExportService exportService;
        private List<Item> currentItems;
        private ChartService.ChartType currentChartType = ChartService.ChartType.Column;
        private bool isGroupedByCategory = false;

        public MainForm()
        {
            InitializeComponent();
            chartService = new ChartService();
            exportService = new ExportService();
            currentItems = new List<Item>();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            InitializeRepository();
            await LoadDataAsync();
            UpdateStatusBar("Ready");
        }

        private void InitializeRepository()
        {
            try
            {
                string connectionString = ConfigHelper.GetConnectionString();
                repository = new ItemRepository(connectionString);
                UpdateConnectionStatus(true);
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false);
                MessageBox.Show($"Failed to initialize database connection:\n{ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                UpdateStatusBar("Loading data...");
                toolStripProgressBar.Visible = true;

                currentItems = await repository.GetAllItemsAsync();
                
                // Update DataGridView
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = currentItems;
                FormatDataGridView();

                // Update Chart
                RefreshChart();

                // Update Statistics
                await UpdateStatisticsAsync();

                UpdateStatusBar($"Loaded {currentItems.Count} items");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatusBar("Error loading data");
            }
            finally
            {
                toolStripProgressBar.Visible = false;
            }
        }

        private void RefreshChart()
        {
            if (currentItems == null || currentItems.Count == 0)
            {
                chart1.Series.Clear();
                chart1.Titles.Clear();
                return;
            }

            if (isGroupedByCategory)
            {
                chartService.ConfigureChartByCategory(chart1, currentItems, currentChartType);
            }
            else
            {
                chartService.ConfigureChart(chart1, currentItems, currentChartType, "Item Price Analysis");
            }
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                var stats = await repository.GetStatisticsAsync();
                
                lblTotalItems.Text = $"Total Items: {stats["Total"]:F0}";
                lblTotalValue.Text = $"Total Value: ${stats["Sum"]:F2}";
                lblAverage.Text = $"Average: ${stats["Average"]:F2}";
                lblMinMax.Text = $"Min: ${stats["Minimum"]:F2} | Max: ${stats["Maximum"]:F2}";
            }
            catch (Exception ex)
            {
                lblTotalItems.Text = "Statistics unavailable";
                lblTotalValue.Text = "";
                lblAverage.Text = "";
                lblMinMax.Text = "";
            }
        }

        private void FormatDataGridView()
        {
            if (dataGridView1.Columns.Count == 0) return;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.Fixed3D;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            // Column headers
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.EnableHeadersVisualStyles = false;

            // Format specific columns
            if (dataGridView1.Columns["Price"] != null)
            {
                dataGridView1.Columns["Price"].DefaultCellStyle.Format = "C2";
                dataGridView1.Columns["Price"].DefaultCellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                dataGridView1.Columns["Price"].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            if (dataGridView1.Columns["CreatedAt"] != null)
            {
                dataGridView1.Columns["CreatedAt"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }

            if (dataGridView1.Columns["UpdatedAt"] != null)
            {
                dataGridView1.Columns["UpdatedAt"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }

            // Hide ID column if needed
            if (dataGridView1.Columns["ID"] != null)
            {
                dataGridView1.Columns["ID"].Width = 50;
            }
        }

        private void UpdateStatusBar(string message)
        {
            toolStripStatusLabel.Text = message;
        }

        private void UpdateConnectionStatus(bool connected)
        {
            toolStripStatusConnection.Text = connected ? "Connected" : "Disconnected";
            toolStripStatusConnection.ForeColor = connected ? Color.Green : Color.Red;
        }

        // Menu Event Handlers
        private async void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void addItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new ItemEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await repository.AddItemAsync(form.Item);
                        await LoadDataAsync();
                        MessageBox.Show("Item added successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding item:\n{ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void editItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to edit.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = (Item)dataGridView1.SelectedRows[0].DataBoundItem;
            
            using (var form = new ItemEditForm(selectedItem))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await repository.UpdateItemAsync(form.Item);
                        await LoadDataAsync();
                        MessageBox.Show("Item updated successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating item:\n{ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to delete.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = (Item)dataGridView1.SelectedRows[0].DataBoundItem;
            
            var result = MessageBox.Show($"Are you sure you want to delete '{selectedItem.Name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await repository.DeleteItemAsync(selectedItem.ID);
                    await LoadDataAsync();
                    MessageBox.Show("Item deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item:\n{ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Chart Type Menu Handlers
        private void columnChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Column;
            RefreshChart();
        }

        private void barChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Bar;
            RefreshChart();
        }

        private void lineChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Line;
            RefreshChart();
        }

        private void pieChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Pie;
            RefreshChart();
        }

        private void areaChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Area;
            RefreshChart();
        }

        private void splineChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentChartType = ChartService.ChartType.Spline;
            RefreshChart();
        }

        private void groupByCategoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isGroupedByCategory = !isGroupedByCategory;
            groupByCategoryToolStripMenuItem.Checked = isGroupedByCategory;
            RefreshChart();
        }

        // Export Menu Handlers
        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"items_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        exportService.ExportToCSV(currentItems, sfd.FileName);
                        MessageBox.Show("Data exported successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting data:\n{ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void exportToHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "HTML Files (*.html)|*.html";
                sfd.FileName = $"items_report_{DateTime.Now:yyyyMMdd_HHmmss}.html";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var stats = await repository.GetStatisticsAsync();
                        exportService.ExportToHTML(currentItems, sfd.FileName, stats);
                        MessageBox.Show("Report exported successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Ask to open
                        if (MessageBox.Show("Would you like to open the report?", "Open Report", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(sfd.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting report:\n{ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void exportToJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON Files (*.json)|*.json";
                sfd.FileName = $"items_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        exportService.ExportToJSON(currentItems, sfd.FileName);
                        MessageBox.Show("Data exported successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting data:\n{ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Reinitialize repository with new connection
                    repository?.Dispose();
                    InitializeRepository();
                    LoadDataAsync();
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new AboutForm())
            {
                form.ShowDialog();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGridView1.DataSource = currentItems;
            }
            else
            {
                var filtered = currentItems.Where(i => 
                    i.Name.ToLower().Contains(searchText) ||
                    i.Category.ToLower().Contains(searchText) ||
                    i.Description.ToLower().Contains(searchText) ||
                    i.Price.ToString().Contains(searchText)
                ).ToList();
                
                dataGridView1.DataSource = filtered;
            }
            
            FormatDataGridView();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                editItemToolStripMenuItem_Click(sender, e);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            repository?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
