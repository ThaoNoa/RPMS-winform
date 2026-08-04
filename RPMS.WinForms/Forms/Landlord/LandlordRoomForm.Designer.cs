using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordRoomForm
    {
        private System.ComponentModel.IContainer components = null;
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
            this.cboHouses = new ComboBox();
            this.btnAddRoom = UIHelper.PrimaryButton("+ Thêm Phòng mới", 160);
            this.dgvRooms = new ModernDataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).BeginInit();
            this.SuspendLayout();

            UIHelper.StyleCombo(this.cboHouses);
            this.cboHouses.SelectedIndexChanged += new System.EventHandler(this.cboHouses_SelectedIndexChanged);

            this.btnAddRoom.Click += new System.EventHandler(this.btnAddRoom_Click);
            this.dgvRooms.CellContentClick += new DataGridViewCellEventHandler(this.dgvRooms_CellContentClick);

            var header = UIHelper.CreatePageHeader("Quản lý Phòng trọ", this.btnAddRoom);
            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Chọn nhà", this.cboHouses, 280));

            this.ClientSize = new Size(900, 600);
            this.Text = "Quản lý Phòng trọ";
            this.Controls.Add(this.dgvRooms);
            this.Controls.Add(filterBar);
            this.Controls.Add(header);
            UIHelper.WireListPage(this, header, this.dgvRooms);
            UIHelper.ApplyGridFill(this.dgvRooms);

            ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
