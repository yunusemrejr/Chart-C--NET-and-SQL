using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Forms
{
    public partial class ItemEditForm : Form
    {
        public Item Item { get; private set; }
        private bool isEditMode;

        public ItemEditForm(Item item = null)
        {
            InitializeComponent();
            isEditMode = item != null;
            
            if (isEditMode)
            {
                Item = item;
                LoadItemData();
                this.Text = "Edit Item";
            }
            else
            {
                Item = new Item();
                this.Text = "Add New Item";
            }
        }

        private void LoadItemData()
        {
            txtName.Text = Item.Name;
            numPrice.Value = Item.Price;
            txtCategory.Text = Item.Category;
            txtDescription.Text = Item.Description;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            Item.Name = txtName.Text.Trim();
            Item.Price = numPrice.Value;
            Item.Category = txtCategory.Text.Trim();
            Item.Description = txtDescription.Text.Trim();
            Item.UpdatedAt = DateTime.Now;

            if (!isEditMode)
            {
                Item.CreatedAt = DateTime.Now;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Name is required");
                txtName.Focus();
                return false;
            }

            if (txtName.Text.Trim().Length < 2)
            {
                errorProvider1.SetError(txtName, "Name must be at least 2 characters");
                txtName.Focus();
                return false;
            }

            if (numPrice.Value <= 0)
            {
                errorProvider1.SetError(numPrice, "Price must be greater than 0");
                numPrice.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                errorProvider1.SetError(txtCategory, "Category is required");
                txtCategory.Focus();
                return false;
            }

            return true;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.SetError(txtName, "");
        }

        private void numPrice_ValueChanged(object sender, EventArgs e)
        {
            errorProvider1.SetError(numPrice, "");
        }

        private void txtCategory_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.SetError(txtCategory, "");
        }
    }
}
