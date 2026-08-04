using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class UserManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private ModernTextBox txtSearch;
        private ModernButton btnSearch;
        private ModernButton btnAdd;
        private ModernDataGridView dgvUsers;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtSearch = new ModernTextBox();
            this.btnSearch = UIHelper.SecondaryButton("Tìm kiếm");
            this.btnAdd = UIHelper.PrimaryButton("+ Thêm mới", 130);
            this.dgvUsers = new ModernDataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            this.txtSearch.Width = 280;
            this.txtSearch.Height = AppLayout.InputHeight;
            this.txtSearch.PlaceholderText = "Tên, username, email…";

            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            this.dgvUsers.CellContentClick += new DataGridViewCellEventHandler(this.dgvUsers_CellContentClick);

            var header = UIHelper.CreatePageHeader("Quản lý người dùng", this.btnAdd);
            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Tìm kiếm", this.txtSearch, 280));
            this.btnSearch.Margin = new Padding(0, 18, AppLayout.FieldGap, 6);
            filterBar.Controls.Add(this.btnSearch);

            this.ClientSize = new Size(900, 600);
            this.Text = "Quản lý người dùng";
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(filterBar);
            this.Controls.Add(header);
            UIHelper.WireListPage(this, header, this.dgvUsers);
            UIHelper.ApplyGridFill(this.dgvUsers);

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
