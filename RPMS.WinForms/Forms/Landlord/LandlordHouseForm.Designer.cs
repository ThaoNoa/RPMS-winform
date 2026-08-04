using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordHouseForm
    {
        private System.ComponentModel.IContainer components = null;
        private ModernButton btnAdd;
        private ModernDataGridView dgvHouses;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnAdd = UIHelper.PrimaryButton("+ Thêm Nhà mới", 150);
            this.dgvHouses = new ModernDataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvHouses)).BeginInit();
            this.SuspendLayout();

            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            this.dgvHouses.CellContentClick += new DataGridViewCellEventHandler(this.dgvHouses_CellContentClick);

            var header = UIHelper.CreatePageHeader("Quản lý Nhà của tôi", this.btnAdd);

            this.ClientSize = new Size(900, 600);
            this.Text = "Quản lý Nhà của tôi";
            this.Controls.Add(this.dgvHouses);
            this.Controls.Add(header);
            UIHelper.WireListPage(this, header, this.dgvHouses);
            UIHelper.ApplyGridFill(this.dgvHouses);

            ((System.ComponentModel.ISupportInitialize)(this.dgvHouses)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
