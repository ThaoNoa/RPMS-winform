using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class UserManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlTop;
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
            this.pnlTop = new Panel();
            this.txtSearch = new ModernTextBox();
            this.btnSearch = new ModernButton();
            this.btnAdd = new ModernButton();
            this.dgvUsers = new ModernDataGridView();

            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            // pnlTop
            this.pnlTop.BackColor = AppColors.Background;
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 70;
            this.pnlTop.Controls.Add(this.txtSearch);
            this.pnlTop.Controls.Add(this.btnSearch);
            this.pnlTop.Controls.Add(this.btnAdd);

            // txtSearch
            this.txtSearch.Location = new Point(0, 15);
            this.txtSearch.Size = new Size(300, 40);

            // btnSearch
            this.btnSearch.Location = new Point(310, 15);
            this.btnSearch.Size = new Size(100, 38);
            this.btnSearch.Text = "Tìm kiếm";
            this.btnSearch.BackColor = AppColors.Secondary;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);

            // btnAdd
            this.btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnAdd.Location = new Point(780, 15);
            this.btnAdd.Size = new Size(120, 38);
            this.btnAdd.Text = "+ Thêm mới";
            this.btnAdd.BackColor = AppColors.Success;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // dgvUsers
            this.dgvUsers.Dock = DockStyle.Fill;
            this.dgvUsers.Location = new Point(0, 70);
            this.dgvUsers.CellContentClick += new DataGridViewCellEventHandler(this.dgvUsers_CellContentClick);

            // UserManagementForm
            this.ClientSize = new Size(900, 600);
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(this.pnlTop);
            this.BackColor = AppColors.Background;
            this.Text = "Quản lý người dùng";

            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
        }
    }
}