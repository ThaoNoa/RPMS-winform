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
            cboHouses = new ComboBox();
            btnAddRoom = UIHelper.PrimaryButton("+ Thêm Phòng mới", 170);
            dgvRooms = new ModernDataGridView();

            ((System.ComponentModel.ISupportInitialize)dgvRooms).BeginInit();
            SuspendLayout();

            UIHelper.StyleCombo(cboHouses);
            cboHouses.SelectedIndexChanged += cboHouses_SelectedIndexChanged;
            btnAddRoom.Click += btnAddRoom_Click;
            dgvRooms.CellContentClick += dgvRooms_CellContentClick;

            var header = UIHelper.CreatePageHeader("Quản lý Phòng trọ", btnAddRoom);
            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Chọn nhà", cboHouses, 320));

            var pageTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppColors.Card,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            pageTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pageTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pageTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            header.Dock = DockStyle.Fill;
            filterBar.Dock = DockStyle.Fill;
            pageTop.Controls.Add(header, 0, 0);
            pageTop.Controls.Add(filterBar, 0, 1);

            ClientSize = new Size(960, 640);
            Text = "Quản lý Phòng trọ";
            UIHelper.WirePage(this, dgvRooms, pageTop);
            UIHelper.ApplyGridFill(dgvRooms);

            ((System.ComponentModel.ISupportInitialize)dgvRooms).EndInit();
            ResumeLayout(false);
        }
    }
}
