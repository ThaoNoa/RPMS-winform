using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class PostManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlTop;
        private ComboBox cboStatusFilter;
        private ModernButton btnRefresh;
        private ModernDataGridView dgvPosts;

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
            this.cboStatusFilter = new ComboBox();
            this.btnRefresh = new ModernButton();
            this.dgvPosts = new ModernDataGridView();

            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPosts)).BeginInit();
            this.SuspendLayout();

            // pnlTop
            this.pnlTop.BackColor = AppColors.Background;
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 70;
            this.pnlTop.Controls.Add(this.cboStatusFilter);
            this.pnlTop.Controls.Add(this.btnRefresh);

            // cboStatusFilter
            this.cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatusFilter.Font = new Font("Segoe UI", 11F);
            this.cboStatusFilter.Location = new Point(20, 18);
            this.cboStatusFilter.Size = new Size(200, 28);
            this.cboStatusFilter.Items.AddRange(new object[] { "Chờ duyệt", "Đã duyệt" });
            this.cboStatusFilter.SelectedIndex = 0;
            this.cboStatusFilter.SelectedIndexChanged += new System.EventHandler(this.cboStatusFilter_SelectedIndexChanged);

            // btnRefresh
            this.btnRefresh.Location = new Point(240, 15);
            this.btnRefresh.Size = new Size(100, 38);
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.BackColor = AppColors.Secondary;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // dgvPosts
            this.dgvPosts.Dock = DockStyle.Fill;
            this.dgvPosts.Location = new Point(0, 70);
            this.dgvPosts.CellContentClick += new DataGridViewCellEventHandler(this.dgvPosts_CellContentClick);

            // PostManagementForm
            this.ClientSize = new Size(1000, 600);
            this.Controls.Add(this.dgvPosts);
            this.Controls.Add(this.pnlTop);
            this.BackColor = AppColors.Background;
            this.Text = "Quản lý tin đăng";

            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPosts)).EndInit();
            this.ResumeLayout(false);
        }
    }
}