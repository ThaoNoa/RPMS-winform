using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordRoomForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlTop;
        private Label lblSelectHouse;
        private ComboBox cboHouses;
        private ModernButton btnAddRoom;
        private ModernDataGridView dgvRooms;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlTop = new Panel();
            this.lblSelectHouse = new Label();
            this.cboHouses = new ComboBox();
            this.btnAddRoom = new ModernButton();
            this.dgvRooms = new ModernDataGridView();

            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).BeginInit();
            this.SuspendLayout();

            // pnlTop
            this.pnlTop.BackColor = AppColors.Background;
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 70;
            this.pnlTop.Controls.Add(this.lblSelectHouse);
            this.pnlTop.Controls.Add(this.cboHouses);
            this.pnlTop.Controls.Add(this.btnAddRoom);

            // lblSelectHouse
            this.lblSelectHouse.AutoSize = true;
            this.lblSelectHouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblSelectHouse.Location = new Point(20, 25);
            this.lblSelectHouse.Text = "Chọn nhà:";

            // cboHouses
            this.cboHouses.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboHouses.Font = new Font("Segoe UI", 11F);
            this.cboHouses.Location = new Point(100, 21);
            this.cboHouses.Size = new Size(250, 28);
            this.cboHouses.SelectedIndexChanged += new System.EventHandler(this.cboHouses_SelectedIndexChanged);

            // btnAddRoom
            this.btnAddRoom.Location = new Point(370, 15);
            this.btnAddRoom.Size = new Size(150, 40);
            this.btnAddRoom.Text = "+ Thêm Phòng mới";
            this.btnAddRoom.BackColor = AppColors.Success;
            this.btnAddRoom.Click += new System.EventHandler(this.btnAddRoom_Click);

            // dgvRooms
            this.dgvRooms.Dock = DockStyle.Fill;
            this.dgvRooms.Location = new Point(0, 70);
            this.dgvRooms.CellContentClick += new DataGridViewCellEventHandler(this.dgvRooms_CellContentClick);

            // LandlordRoomForm
            this.ClientSize = new Size(900, 600);
            this.Controls.Add(this.dgvRooms);
            this.Controls.Add(this.pnlTop);
            this.BackColor = AppColors.Background;
            this.Text = "Quản lý Phòng trọ";

            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).EndInit();
            this.ResumeLayout(false);
        }
    }
}