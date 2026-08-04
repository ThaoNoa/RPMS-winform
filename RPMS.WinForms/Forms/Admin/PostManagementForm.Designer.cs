using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class PostManagementForm
    {
        private System.ComponentModel.IContainer components = null;
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
            this.cboStatusFilter = new ComboBox();
            this.btnRefresh = UIHelper.SecondaryButton("Làm mới");
            this.dgvPosts = new ModernDataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvPosts)).BeginInit();
            this.SuspendLayout();

            UIHelper.StyleCombo(this.cboStatusFilter);
            this.cboStatusFilter.Items.AddRange(new object[] { "Chờ duyệt", "Đã duyệt" });
            this.cboStatusFilter.SelectedIndex = 0;
            this.cboStatusFilter.SelectedIndexChanged += new System.EventHandler(this.cboStatusFilter_SelectedIndexChanged);

            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.dgvPosts.CellContentClick += new DataGridViewCellEventHandler(this.dgvPosts_CellContentClick);

            var header = UIHelper.CreatePageHeader("Quản lý tin đăng", this.btnRefresh);
            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Trạng thái", this.cboStatusFilter, 200));

            this.ClientSize = new Size(1000, 600);
            this.Text = "Quản lý tin đăng";
            this.Controls.Add(this.dgvPosts);
            this.Controls.Add(filterBar);
            this.Controls.Add(header);
            UIHelper.WireListPage(this, header, this.dgvPosts);
            UIHelper.ApplyGridFill(this.dgvPosts);

            ((System.ComponentModel.ISupportInitialize)(this.dgvPosts)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
