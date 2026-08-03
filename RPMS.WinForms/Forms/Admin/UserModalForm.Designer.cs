using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
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
            this.btnSave = new ModernButton();
            this.btnCancel = new ModernButton();

            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Text = "Thông tin Người dùng";
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = AppColors.TextMain;
            this.lblTitle.Location = new Point(30, 20);
            this.lblTitle.AutoSize = true;

            int startX = 30, startY = 80, gapY = 70;

            // Username
            this.lblUsername.Text = "Tên đăng nhập *";
            this.lblUsername.Location = new Point(startX, startY);
            this.lblUsername.AutoSize = true;
            this.txtUsername.Location = new Point(startX, startY + 20);
            this.txtUsername.Size = new Size(250, 35);

            // Password
            this.lblPassword.Text = "Mật khẩu *";
            this.lblPassword.Location = new Point(startX + 280, startY);
            this.lblPassword.AutoSize = true;
            this.txtPassword.Location = new Point(startX + 280, startY + 20);
            this.txtPassword.Size = new Size(250, 35);

            // FullName
            this.lblFullName.Text = "Họ và tên *";
            this.lblFullName.Location = new Point(startX, startY + gapY);
            this.lblFullName.AutoSize = true;
            this.txtFullName.Location = new Point(startX, startY + gapY + 20);
            this.txtFullName.Size = new Size(250, 35);

            // Email
            this.lblEmail.Text = "Email *";
            this.lblEmail.Location = new Point(startX + 280, startY + gapY);
            this.lblEmail.AutoSize = true;
            this.txtEmail.Location = new Point(startX + 280, startY + gapY + 20);
            this.txtEmail.Size = new Size(250, 35);

            // Phone
            this.lblPhone.Text = "Số điện thoại";
            this.lblPhone.Location = new Point(startX, startY + gapY * 2);
            this.lblPhone.AutoSize = true;
            this.txtPhone.Location = new Point(startX, startY + gapY * 2 + 20);
            this.txtPhone.Size = new Size(250, 35);

            // Address
            this.lblAddress.Text = "Địa chỉ";
            this.lblAddress.Location = new Point(startX + 280, startY + gapY * 2);
            this.lblAddress.AutoSize = true;
            this.txtAddress.Location = new Point(startX + 280, startY + gapY * 2 + 20);
            this.txtAddress.Size = new Size(250, 35);

            // Role
            this.lblRole.Text = "Vai trò *";
            this.lblRole.Location = new Point(startX, startY + gapY * 3);
            this.lblRole.AutoSize = true;
            this.cboRole.Location = new Point(startX, startY + gapY * 3 + 20);
            this.cboRole.Size = new Size(250, 35);
            this.cboRole.DropDownStyle = ComboBoxStyle.DropDownList;

            // Status
            this.lblStatus.Text = "Trạng thái *";
            this.lblStatus.Location = new Point(startX + 280, startY + gapY * 3);
            this.lblStatus.AutoSize = true;
            this.cboStatus.Location = new Point(startX + 280, startY + gapY * 3 + 20);
            this.cboStatus.Size = new Size(250, 35);
            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });

            // Buttons
            this.btnSave.Text = "Lưu thông tin";
            this.btnSave.Location = new Point(startX + 280, startY + gapY * 4 + 10);
            this.btnSave.Size = new Size(120, 40);
            this.btnSave.BackColor = AppColors.Success;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Text = "Hủy bỏ";
            this.btnCancel.Location = new Point(startX + 410, startY + gapY * 4 + 10);
            this.btnCancel.Size = new Size(120, 40);
            this.btnCancel.BackColor = AppColors.Secondary;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Form
            this.ClientSize = new Size(600, 450);
            this.Controls.AddRange(new Control[] { lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, lblFullName, txtFullName, lblEmail, txtEmail, lblPhone, txtPhone, lblAddress, txtAddress, lblRole, cboRole, lblStatus, cboStatus, btnSave, btnCancel });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppColors.Card;
            this.Text = "Chi tiết Người dùng";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}