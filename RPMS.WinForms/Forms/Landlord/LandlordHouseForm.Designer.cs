using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordHouseForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlTop;
        private ModernButton btnAdd;
        private ModernDataGridView dgvHouses;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlTop = new Panel();
            this.btnAdd = new ModernButton();
            this.dgvHouses = new ModernDataGridView();

            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHouses)).BeginInit();
            this.SuspendLayout();

            // pnlTop
            this.pnlTop.BackColor = AppColors.Background;
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 70;
            this.pnlTop.Controls.Add(this.btnAdd);

            // btnAdd
            this.btnAdd.Location = new Point(20, 15);
            this.btnAdd.Size = new Size(150, 40);
            this.btnAdd.Text = "+ Thêm Nhà mới";
            this.btnAdd.BackColor = AppColors.Success;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // dgvHouses
            this.dgvHouses.Dock = DockStyle.Fill;
            this.dgvHouses.Location = new Point(0, 70);
            this.dgvHouses.CellContentClick += new DataGridViewCellEventHandler(this.dgvHouses_CellContentClick);

            // LandlordHouseForm
            this.ClientSize = new Size(900, 600);
            this.Controls.Add(this.dgvHouses);
            this.Controls.Add(this.pnlTop);
            this.BackColor = AppColors.Background;
            this.Text = "Quản lý Nhà của tôi";

            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHouses)).EndInit();
            this.ResumeLayout(false);
        }
    }
}