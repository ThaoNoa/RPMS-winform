using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordRoomModalForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private TabControl tabMain;
        private TabPage tpGeneral, tpAmenities, tpImages;

        // General Controls
        private Label lblRoomNumber, lblFloor, lblArea, lblPrice, lblCapacity, lblBedroom, lblBathroom, lblStatus, lblFurniture, lblDescription;
        private ModernTextBox txtRoomNumber, txtFloor, txtArea, txtPrice, txtCapacity, txtBedroom, txtBathroom, txtFurniture, txtDescription;
        private ComboBox cboStatus;

        // Amenities Controls
        private CheckedListBox clbAmenities;

        // Images Controls
        private ListBox lstImages;
        private PictureBox picPreview;
        private ModernButton btnAddImage, btnRemoveImage;

        // Action Buttons
        private ModernButton btnSave, btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.tabMain = new TabControl();
            this.tpGeneral = new TabPage();
            this.tpAmenities = new TabPage();
            this.tpImages = new TabPage();

            // Init General
            this.lblRoomNumber = new Label();
            this.txtRoomNumber = new ModernTextBox();
            this.lblFloor = new Label();
            this.txtFloor = new ModernTextBox();
            this.lblArea = new Label();
            this.txtArea = new ModernTextBox();
            this.lblPrice = new Label();
            this.txtPrice = new ModernTextBox();
            this.lblCapacity = new Label();
            this.txtCapacity = new ModernTextBox();
            this.lblBedroom = new Label();
            this.txtBedroom = new ModernTextBox();
            this.lblBathroom = new Label();
            this.txtBathroom = new ModernTextBox();
            this.lblStatus = new Label();
            this.cboStatus = new ComboBox();
            this.lblFurniture = new Label();
            this.txtFurniture = new ModernTextBox();
            this.lblDescription = new Label();
            this.txtDescription = new ModernTextBox();

            // Init Amenities
            this.clbAmenities = new CheckedListBox();

            // Init Images
            this.lstImages = new ListBox();
            this.picPreview = new PictureBox();
            this.btnAddImage = new ModernButton();
            this.btnRemoveImage = new ModernButton();

            // Init Actions
            this.btnSave = new ModernButton();
            this.btnCancel = new ModernButton();

            this.tabMain.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.tpAmenities.SuspendLayout();
            this.tpImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Text = "Thông tin Phòng trọ";
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = AppColors.TextMain;
            this.lblTitle.Location = new Point(20, 15);
            this.lblTitle.AutoSize = true;

            // tabMain
            this.tabMain.Controls.Add(this.tpGeneral);
            this.tabMain.Controls.Add(this.tpAmenities);
            this.tabMain.Controls.Add(this.tpImages);
            this.tabMain.Location = new Point(20, 60);
            this.tabMain.Size = new Size(740, 420);
            this.tabMain.Font = new Font("Segoe UI", 10F);

            // tpGeneral
            this.tpGeneral.Text = "Thông tin";
            this.tpGeneral.BackColor = AppColors.Card;
            int col1 = 20, col2 = 360, startY = 20, gap = 60;

            // RoomNumber
            this.lblRoomNumber.Text = "Mã phòng *";
            this.lblRoomNumber.Location = new Point(col1, startY);
            this.lblRoomNumber.AutoSize = true;
            this.txtRoomNumber.Location = new Point(col1, startY + 20);
            this.txtRoomNumber.Size = new Size(300, 35);

            // Floor
            this.lblFloor.Text = "Tầng";
            this.lblFloor.Location = new Point(col2, startY);
            this.lblFloor.AutoSize = true;
            this.txtFloor.Location = new Point(col2, startY + 20);
            this.txtFloor.Size = new Size(300, 35);

            // Area
            this.lblArea.Text = "Diện tích (m2) *";
            this.lblArea.Location = new Point(col1, startY + gap);
            this.lblArea.AutoSize = true;
            this.txtArea.Location = new Point(col1, startY + gap + 20);
            this.txtArea.Size = new Size(300, 35);

            // Price
            this.lblPrice.Text = "Giá thuê (VND) *";
            this.lblPrice.Location = new Point(col2, startY + gap);
            this.lblPrice.AutoSize = true;
            this.txtPrice.Location = new Point(col2, startY + gap + 20);
            this.txtPrice.Size = new Size(300, 35);

            // Capacity
            this.lblCapacity.Text = "Số người tối đa *";
            this.lblCapacity.Location = new Point(col1, startY + gap * 2);
            this.lblCapacity.AutoSize = true;
            this.txtCapacity.Location = new Point(col1, startY + gap * 2 + 20);
            this.txtCapacity.Size = new Size(140, 35);

            // Bedroom
            this.lblBedroom.Text = "Số phòng ngủ";
            this.lblBedroom.Location = new Point(col2, startY + gap * 2);
            this.lblBedroom.AutoSize = true;
            this.txtBedroom.Location = new Point(col2, startY + gap * 2 + 20);
            this.txtBedroom.Size = new Size(140, 35);

            // Bathroom
            this.lblBathroom.Text = "Số phòng tắm";
            this.lblBathroom.Location = new Point(col2 + 160, startY + gap * 2);
            this.lblBathroom.AutoSize = true;
            this.txtBathroom.Location = new Point(col2 + 160, startY + gap * 2 + 20);
            this.txtBathroom.Size = new Size(140, 35);

            // Status
            this.lblStatus.Text = "Trạng thái *";
            this.lblStatus.Location = new Point(col1, startY + gap * 3);
            this.lblStatus.AutoSize = true;
            this.cboStatus.Location = new Point(col1, startY + gap * 3 + 20);
            this.cboStatus.Size = new Size(300, 35);
            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "Available", "Occupied", "Maintenance" });

            // Furniture
            this.lblFurniture.Text = "Nội thất";
            this.lblFurniture.Location = new Point(col2, startY + gap * 3);
            this.lblFurniture.AutoSize = true;
            this.txtFurniture.Location = new Point(col2, startY + gap * 3 + 20);
            this.txtFurniture.Size = new Size(300, 35);

            // Description
            this.lblDescription.Text = "Mô tả thêm";
            this.lblDescription.Location = new Point(col1, startY + gap * 4);
            this.lblDescription.AutoSize = true;
            this.txtDescription.Location = new Point(col1, startY + gap * 4 + 20);
            this.txtDescription.Size = new Size(640, 70);

            this.tpGeneral.Controls.AddRange(new Control[] {
                lblRoomNumber, txtRoomNumber, lblFloor, txtFloor,
                lblArea, txtArea, lblPrice, txtPrice,
                lblCapacity, txtCapacity, lblBedroom, txtBedroom, lblBathroom, txtBathroom,
                lblStatus, cboStatus,
                lblFurniture, txtFurniture,
                lblDescription, txtDescription
            });

            // tpAmenities
            this.tpAmenities.Text = "Tiện ích";
            this.tpAmenities.BackColor = AppColors.Card;
            this.clbAmenities.Dock = DockStyle.Fill;
            this.clbAmenities.BorderStyle = BorderStyle.None;
            this.clbAmenities.Padding = new Padding(20);
            this.clbAmenities.CheckOnClick = true;
            this.clbAmenities.Font = new Font("Segoe UI", 12F);
            this.tpAmenities.Controls.Add(this.clbAmenities);

            // tpImages
            this.tpImages.Text = "Ảnh & Video";
            this.tpImages.BackColor = AppColors.Card;
            this.lstImages.Location = new Point(20, 20);
            this.lstImages.Size = new Size(300, 320);
            this.lstImages.SelectedIndexChanged += new System.EventHandler(this.lstImages_SelectedIndexChanged);

            this.picPreview.Location = new Point(340, 20);
            this.picPreview.Size = new Size(360, 260);
            this.picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            this.picPreview.BorderStyle = BorderStyle.FixedSingle;

            this.btnAddImage.Text = "Thêm ảnh/video";
            this.btnAddImage.Location = new Point(340, 300);
            this.btnAddImage.Size = new Size(140, 35);
            this.btnAddImage.BackColor = AppColors.Primary;
            this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);

            this.btnRemoveImage.Text = "Xóa ảnh";
            this.btnRemoveImage.Location = new Point(480, 300);
            this.btnRemoveImage.Size = new Size(120, 35);
            this.btnRemoveImage.BackColor = AppColors.Danger;
            this.btnRemoveImage.Click += new System.EventHandler(this.btnRemoveImage_Click);

            this.tpImages.Controls.AddRange(new Control[] { lstImages, picPreview, btnAddImage, btnRemoveImage });

            // Buttons
            this.btnSave.Text = "Lưu thông tin";
            this.btnSave.Location = new Point(490, 500);
            this.btnSave.Size = new Size(120, 40);
            this.btnSave.BackColor = AppColors.Success;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Text = "Hủy bỏ";
            this.btnCancel.Location = new Point(630, 500);
            this.btnCancel.Size = new Size(120, 40);
            this.btnCancel.BackColor = AppColors.Secondary;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Form
            this.ClientSize = new Size(780, 570);
            this.MinimumSize = new Size(480, 400);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.AutoScroll = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppColors.Background;
            this.Text = "Chi tiết Phòng";

            this.tabMain.ResumeLayout(false);
            this.tpGeneral.ResumeLayout(false);
            this.tpGeneral.PerformLayout();
            this.tpAmenities.ResumeLayout(false);
            this.tpImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}