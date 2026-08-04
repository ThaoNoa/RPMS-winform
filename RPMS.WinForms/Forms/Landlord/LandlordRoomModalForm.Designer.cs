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

        private Label lblRoomNumber, lblFloor, lblArea, lblPrice, lblCapacity, lblBedroom, lblBathroom, lblStatus, lblFurniture, lblDescription;
        private ModernTextBox txtRoomNumber, txtFloor, txtArea, txtPrice, txtCapacity, txtBedroom, txtBathroom, txtFurniture;
        private TextBox txtDescription;
        private ComboBox cboStatus;

        private CheckedListBox clbAmenities;

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

            lblRoomNumber = new Label();
            txtRoomNumber = new ModernTextBox();
            lblFloor = new Label();
            txtFloor = new ModernTextBox();
            lblArea = new Label();
            txtArea = new ModernTextBox();
            lblPrice = new Label();
            txtPrice = new ModernTextBox();
            lblCapacity = new Label();
            txtCapacity = new ModernTextBox();
            lblBedroom = new Label();
            txtBedroom = new ModernTextBox();
            lblBathroom = new Label();
            txtBathroom = new ModernTextBox();
            lblStatus = new Label();
            cboStatus = new ComboBox();
            lblFurniture = new Label();
            txtFurniture = new ModernTextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();

            clbAmenities = new CheckedListBox();

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

            // ===== General (scroll + flow so nothing is clipped) =====
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

            int row = 0;
            void AddRow(Control left, Control right)
            {
                generalStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                generalStack.Controls.Add(left, 0, row);
                if (right != null)
                    generalStack.Controls.Add(right, 1, row);
                else
                    generalStack.SetColumnSpan(left, 2);
                row++;
            }

            AddRow(
                UIHelper.CreateLabeledField("Mã phòng *", txtRoomNumber, 280),
                UIHelper.CreateLabeledField("Tầng", txtFloor, 280));
            AddRow(
                UIHelper.CreateLabeledField("Diện tích (m²) *", txtArea, 280),
                UIHelper.CreateLabeledField("Giá thuê (VND) *", txtPrice, 280));
            AddRow(
                UIHelper.CreateLabeledField("Số người tối đa *", txtCapacity, 280),
                UIHelper.CreateLabeledField("Số phòng ngủ", txtBedroom, 280));

            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.AddRange(new object[] { "Available", "Occupied", "Maintenance" });
            UIHelper.StyleCombo(cboStatus);
            AddRow(
                UIHelper.CreateLabeledField("Số phòng tắm", txtBathroom, 280),
                UIHelper.CreateLabeledField("Trạng thái *", cboStatus, 280));
            AddRow(UIHelper.CreateLabeledField("Nội thất", txtFurniture, 600), null!);

            lblDescription.Text = "Mô tả thêm";
            lblDescription.Font = AppTypography.Caption;
            lblDescription.ForeColor = AppColors.TextMuted;
            lblDescription.AutoSize = true;
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Height = 90;
            txtDescription.Width = 600;
            txtDescription.Font = AppTypography.Body;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;
            var descWrap = new Panel
            {
                Width = 620,
                Height = 120,
                Margin = new Padding(0, 0, AppLayout.FieldGap, 6)
            };
            lblDescription.Location = new Point(0, 0);
            txtDescription.Location = new Point(0, 18);
            descWrap.Controls.Add(lblDescription);
            descWrap.Controls.Add(txtDescription);
            AddRow(descWrap, null!);

            tpGeneral.Controls.Add(generalStack);

            // ===== Amenities =====
            tpAmenities.Text = "Tiện ích";
            tpAmenities.BackColor = AppColors.Card;
            tpAmenities.Padding = new Padding(AppLayout.PagePadding);
            clbAmenities.Dock = DockStyle.Fill;
            clbAmenities.BorderStyle = BorderStyle.FixedSingle;
            clbAmenities.CheckOnClick = true;
            clbAmenities.Font = AppTypography.Body;
            tpAmenities.Controls.Add(clbAmenities);

            // ===== Images — docked layout, upload always visible =====
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

            lblImageHint.Text = "Chọn tab này → bấm «Thêm ảnh/video» để tải file từ máy. Có thể chọn nhiều file.";
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
