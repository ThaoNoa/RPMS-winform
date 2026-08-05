using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
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

        private ModernTextBox txtRoomNumber, txtFloor, txtArea, txtPrice, txtCapacity, txtBedroom, txtBathroom, txtFurniture;
        private TextBox txtDescription;
        private ComboBox cboStatus;

        private FlowLayoutPanel flpAmenities;
        private Label lblAmenitiesHint;

        private ListBox lstImages;
        private PictureBox picPreview;
        private Label lblImageHint;
        private ModernButton btnAddImage, btnRemoveImage;

        private ModernButton btnSave, btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            tabMain = new TabControl();
            tpGeneral = new TabPage();
            tpAmenities = new TabPage();
            tpImages = new TabPage();

            txtRoomNumber = new ModernTextBox();
            txtFloor = new ModernTextBox();
            txtArea = new ModernTextBox();
            txtPrice = new ModernTextBox();
            txtCapacity = new ModernTextBox();
            txtBedroom = new ModernTextBox();
            txtBathroom = new ModernTextBox();
            cboStatus = new ComboBox();
            txtFurniture = new ModernTextBox();
            txtDescription = new TextBox();

            lblAmenitiesHint = new Label();
            flpAmenities = new FlowLayoutPanel();

            lstImages = new ListBox();
            picPreview = new PictureBox();
            lblImageHint = new Label();
            btnAddImage = new ModernButton();
            btnRemoveImage = new ModernButton();

            btnSave = UIHelper.PrimaryButton("Lưu thông tin", 140);
            btnCancel = UIHelper.SecondaryButton("Hủy bỏ");

            tabMain.SuspendLayout();
            tpGeneral.SuspendLayout();
            tpAmenities.SuspendLayout();
            tpImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();

            // Header
            lblTitle.Text = "Thông tin Phòng trọ";
            lblTitle.Font = AppTypography.Heading;
            lblTitle.ForeColor = AppColors.TextMain;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Padding = new Padding(AppLayout.PagePadding, 0, 0, 0);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppLayout.PageHeaderHeight,
                BackColor = AppColors.Card,
                Padding = new Padding(0, 8, 0, 8)
            };
            header.Controls.Add(lblTitle);

            // Tabs
            tabMain.Controls.Add(tpGeneral);
            tabMain.Controls.Add(tpAmenities);
            tabMain.Controls.Add(tpImages);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = AppTypography.Body;
            tabMain.Padding = new Point(8, 8);

            // ===== General =====
            tpGeneral.Text = "Thông tin";
            tpGeneral.BackColor = AppColors.Card;
            tpGeneral.AutoScroll = true;
            tpGeneral.Padding = new Padding(AppLayout.PagePadding);

            var generalStack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Padding = new Padding(4),
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };
            generalStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            generalStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            generalStack.MinimumSize = new Size(680, 0);

            int row = 0;
            void AddFull(Control field, float rowHeight = 62f)
            {
                generalStack.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
                generalStack.Controls.Add(field, 0, row);
                generalStack.SetColumnSpan(field, 2);
                row++;
            }
            void AddRow(Control left, Control right, float rowHeight = 62f)
            {
                generalStack.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
                generalStack.Controls.Add(left, 0, row);
                generalStack.Controls.Add(right, 1, row);
                row++;
            }

            AddRow(
                UIHelper.CreateDialogField("Mã phòng *", txtRoomNumber),
                UIHelper.CreateDialogField("Tầng", txtFloor));
            AddRow(
                UIHelper.CreateDialogField("Diện tích (m²) *", txtArea),
                UIHelper.CreateDialogField("Giá thuê (VND) *", txtPrice));
            AddRow(
                UIHelper.CreateDialogField("Số người tối đa *", txtCapacity),
                UIHelper.CreateDialogField("Số phòng ngủ", txtBedroom));

            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.AddRange(new object[] { "Available", "Occupied", "Maintenance" });
            AddRow(
                UIHelper.CreateDialogField("Số phòng tắm", txtBathroom),
                UIHelper.CreateDialogField("Trạng thái *", cboStatus));
            AddFull(UIHelper.CreateDialogField("Nội thất", txtFurniture));

            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Font = AppTypography.Body;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;
            txtDescription.AcceptsReturn = true;
            AddFull(UIHelper.CreateDialogField("Mô tả thêm", txtDescription, 120), 120);

            tpGeneral.Controls.Add(generalStack);

            // ===== Amenities — checkbox panel =====
            tpAmenities.Text = "Tiện ích";
            tpAmenities.BackColor = AppColors.Card;
            tpAmenities.Padding = new Padding(AppLayout.PagePadding);
            tpAmenities.AutoScroll = true;

            var amenitiesRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            amenitiesRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            amenitiesRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            amenitiesRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            lblAmenitiesHint.Text = "Chọn các tiện ích có trong phòng (điều hòa, máy giặt, tủ lạnh, …).";
            lblAmenitiesHint.Font = AppTypography.Caption;
            lblAmenitiesHint.ForeColor = AppColors.TextMuted;
            lblAmenitiesHint.AutoSize = true;
            lblAmenitiesHint.Dock = DockStyle.Fill;
            lblAmenitiesHint.Padding = new Padding(0, 0, 0, 8);
            amenitiesRoot.Controls.Add(lblAmenitiesHint, 0, 0);

            flpAmenities.Dock = DockStyle.Fill;
            flpAmenities.AutoScroll = true;
            flpAmenities.WrapContents = true;
            flpAmenities.FlowDirection = FlowDirection.LeftToRight;
            flpAmenities.Padding = new Padding(4);
            flpAmenities.BackColor = AppColors.Card;
            amenitiesRoot.Controls.Add(flpAmenities, 0, 1);

            tpAmenities.Controls.Add(amenitiesRoot);

            // ===== Images =====
            tpImages.Text = "Ảnh & Video";
            tpImages.BackColor = AppColors.Card;
            tpImages.Padding = new Padding(AppLayout.PagePadding);

            var imagesRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0)
            };
            imagesRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            imagesRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            imagesRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            imagesRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            imagesRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            lblImageHint.Text = "Bấm «Thêm ảnh/video» để tải file từ máy. Có thể chọn nhiều file.";
            lblImageHint.Font = AppTypography.Caption;
            lblImageHint.ForeColor = AppColors.TextMuted;
            lblImageHint.AutoSize = true;
            lblImageHint.Dock = DockStyle.Fill;
            lblImageHint.Padding = new Padding(0, 0, 0, 8);
            imagesRoot.Controls.Add(lblImageHint, 0, 0);
            imagesRoot.SetColumnSpan(lblImageHint, 2);

            lstImages.Dock = DockStyle.Fill;
            lstImages.IntegralHeight = false;
            lstImages.Font = AppTypography.Body;
            lstImages.SelectedIndexChanged += lstImages_SelectedIndexChanged;
            imagesRoot.Controls.Add(lstImages, 0, 1);

            picPreview.Dock = DockStyle.Fill;
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.BackColor = Color.WhiteSmoke;
            imagesRoot.Controls.Add(picPreview, 1, 1);

            var btnBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            btnAddImage.Text = "Thêm ảnh/video";
            btnAddImage.Size = new Size(160, 36);
            btnAddImage.BackColor = AppColors.Primary;
            btnAddImage.Click += btnAddImage_Click;
            btnRemoveImage.Text = "Xóa đã chọn";
            btnRemoveImage.Size = new Size(130, 36);
            btnRemoveImage.BackColor = AppColors.Danger;
            btnRemoveImage.Click += btnRemoveImage_Click;
            btnBar.Controls.Add(btnAddImage);
            btnBar.Controls.Add(btnRemoveImage);
            imagesRoot.Controls.Add(btnBar, 0, 2);
            imagesRoot.SetColumnSpan(btnBar, 2);

            tpImages.Controls.Add(imagesRoot);

            var footer = UIHelper.CreateDialogFooter(btnSave, btnCancel);
            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;

            ClientSize = new Size(860, 640);
            MinimumSize = new Size(760, 560);
            Controls.Add(tabMain);
            Controls.Add(footer);
            Controls.Add(header);
            UIHelper.ApplyResizableDialog(this, new Size(760, 560));
            Text = "Chi tiết Phòng";
            StartPosition = FormStartPosition.CenterParent;

            tabMain.ResumeLayout(false);
            tpGeneral.ResumeLayout(false);
            tpAmenities.ResumeLayout(false);
            tpImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
        }
    }
}
