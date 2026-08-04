using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordHouseModalForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblName, lblAddress, lblDescription, lblStatus;
        private ModernTextBox txtName, txtAddress, txtDescription;
        private ComboBox cboStatus;
        private ModernButton btnSave, btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblName = new Label();
            this.txtName = new ModernTextBox();
            this.lblAddress = new Label();
            this.txtAddress = new ModernTextBox();
            this.lblDescription = new Label();
            this.txtDescription = new ModernTextBox();
            this.lblStatus = new Label();
            this.cboStatus = new ComboBox();
            this.btnSave = UIHelper.PrimaryButton("Lưu thông tin", 130);
            this.btnCancel = UIHelper.SecondaryButton("Hủy bỏ");

            this.SuspendLayout();

            this.lblTitle.Text = "Thông tin Nhà";
            this.lblTitle.Font = AppTypography.Heading;
            this.lblTitle.ForeColor = AppColors.TextMain;
            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTitle.Padding = new Padding(AppLayout.PagePadding, 0, 0, 0);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppLayout.PageHeaderHeight,
                BackColor = AppColors.Card,
                Padding = new Padding(0, 8, 0, 8)
            };
            header.Controls.Add(this.lblTitle);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(AppLayout.PagePadding),
                BackColor = AppColors.Card
            };

            int startX = AppLayout.PagePadding, startY = AppLayout.PagePadding, gapY = 70;

            this.lblName.Text = "Tên nhà *";
            this.lblName.Location = new Point(startX, startY);
            this.lblName.AutoSize = true;
            this.txtName.Location = new Point(startX, startY + 20);
            this.txtName.Size = new Size(400, 35);
            this.txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.lblAddress.Text = "Địa chỉ *";
            this.lblAddress.Location = new Point(startX, startY + gapY);
            this.lblAddress.AutoSize = true;
            this.txtAddress.Location = new Point(startX, startY + gapY + 20);
            this.txtAddress.Size = new Size(400, 35);
            this.txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.lblDescription.Text = "Mô tả";
            this.lblDescription.Location = new Point(startX, startY + gapY * 2);
            this.lblDescription.AutoSize = true;
            this.txtDescription.Location = new Point(startX, startY + gapY * 2 + 20);
            this.txtDescription.Size = new Size(400, 35);
            this.txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.lblStatus.Text = "Trạng thái *";
            this.lblStatus.Location = new Point(startX, startY + gapY * 3);
            this.lblStatus.AutoSize = true;
            this.cboStatus.Location = new Point(startX, startY + gapY * 3 + 20);
            this.cboStatus.Size = new Size(200, 35);
            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });

            body.Controls.AddRange(new Control[] {
                lblName, txtName, lblAddress, txtAddress, lblDescription, txtDescription, lblStatus, cboStatus
            });

            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            var footer = UIHelper.CreateDialogFooter(this.btnSave, this.btnCancel);

            this.ClientSize = new Size(520, 480);
            this.Controls.Add(body);
            this.Controls.Add(footer);
            this.Controls.Add(header);
            UIHelper.ApplyResizableDialog(this, new Size(480, 420));
            this.Text = "Quản lý Nhà";
            this.AutoScroll = false;

            this.ResumeLayout(false);
        }
    }
}
