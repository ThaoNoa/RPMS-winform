using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class UserModalForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblUsername, lblPassword, lblFullName, lblEmail, lblPhone, lblAddress, lblRole, lblStatus;
        private ModernTextBox txtUsername, txtPassword, txtFullName, txtEmail, txtPhone, txtAddress;
        private ComboBox cboRole, cboStatus;
        private ModernButton btnSave, btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblUsername = new Label();
            this.txtUsername = new ModernTextBox();
            this.lblPassword = new Label();
            this.txtPassword = new ModernTextBox();
            this.lblFullName = new Label();
            this.txtFullName = new ModernTextBox();
            this.lblEmail = new Label();
            this.txtEmail = new ModernTextBox();
            this.lblPhone = new Label();
            this.txtPhone = new ModernTextBox();
            this.lblAddress = new Label();
            this.txtAddress = new ModernTextBox();
            this.lblRole = new Label();
            this.cboRole = new ComboBox();
            this.lblStatus = new Label();
            this.cboStatus = new ComboBox();
            this.btnSave = UIHelper.PrimaryButton("Lưu thông tin", 130);
            this.btnCancel = UIHelper.SecondaryButton("Hủy bỏ");

            this.SuspendLayout();

            this.lblTitle.Text = "Thông tin Người dùng";
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

            this.lblUsername.Text = "Tên đăng nhập *";
            this.lblUsername.Location = new Point(startX, startY);
            this.lblUsername.AutoSize = true;
            this.txtUsername.Location = new Point(startX, startY + 20);
            this.txtUsername.Size = new Size(250, 35);

            this.lblPassword.Text = "Mật khẩu *";
            this.lblPassword.Location = new Point(startX + 280, startY);
            this.lblPassword.AutoSize = true;
            this.txtPassword.Location = new Point(startX + 280, startY + 20);
            this.txtPassword.Size = new Size(250, 35);

            this.lblFullName.Text = "Họ và tên *";
            this.lblFullName.Location = new Point(startX, startY + gapY);
            this.lblFullName.AutoSize = true;
            this.txtFullName.Location = new Point(startX, startY + gapY + 20);
            this.txtFullName.Size = new Size(250, 35);

            this.lblEmail.Text = "Email *";
            this.lblEmail.Location = new Point(startX + 280, startY + gapY);
            this.lblEmail.AutoSize = true;
            this.txtEmail.Location = new Point(startX + 280, startY + gapY + 20);
            this.txtEmail.Size = new Size(250, 35);

            this.lblPhone.Text = "Số điện thoại";
            this.lblPhone.Location = new Point(startX, startY + gapY * 2);
            this.lblPhone.AutoSize = true;
            this.txtPhone.Location = new Point(startX, startY + gapY * 2 + 20);
            this.txtPhone.Size = new Size(250, 35);

            this.lblAddress.Text = "Địa chỉ";
            this.lblAddress.Location = new Point(startX + 280, startY + gapY * 2);
            this.lblAddress.AutoSize = true;
            this.txtAddress.Location = new Point(startX + 280, startY + gapY * 2 + 20);
            this.txtAddress.Size = new Size(250, 35);

            this.lblRole.Text = "Vai trò *";
            this.lblRole.Location = new Point(startX, startY + gapY * 3);
            this.lblRole.AutoSize = true;
            this.cboRole.Location = new Point(startX, startY + gapY * 3 + 20);
            this.cboRole.Size = new Size(250, 35);
            this.cboRole.DropDownStyle = ComboBoxStyle.DropDownList;

            this.lblStatus.Text = "Trạng thái *";
            this.lblStatus.Location = new Point(startX + 280, startY + gapY * 3);
            this.lblStatus.AutoSize = true;
            this.cboStatus.Location = new Point(startX + 280, startY + gapY * 3 + 20);
            this.cboStatus.Size = new Size(250, 35);
            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });

            body.Controls.AddRange(new Control[] {
                lblUsername, txtUsername, lblPassword, txtPassword,
                lblFullName, txtFullName, lblEmail, txtEmail,
                lblPhone, txtPhone, lblAddress, txtAddress,
                lblRole, cboRole, lblStatus, cboStatus
            });

            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            var footer = UIHelper.CreateDialogFooter(this.btnSave, this.btnCancel);

            this.ClientSize = new Size(620, 480);
            this.Controls.Add(body);
            this.Controls.Add(footer);
            this.Controls.Add(header);
            UIHelper.ApplyResizableDialog(this, AppLayout.DialogMin);
            this.Text = "Chi tiết Người dùng";
            this.AutoScroll = false;

            this.ResumeLayout(false);
        }
    }
}
