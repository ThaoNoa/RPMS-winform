using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
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
            this.btnSave = new ModernButton();
            this.btnCancel = new ModernButton();

            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Text = "Thông tin Nhà";
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = AppColors.TextMain;
            this.lblTitle.Location = new Point(30, 20);
            this.lblTitle.AutoSize = true;

            int startX = 30, startY = 80, gapY = 70;

            // Tên nhà
            this.lblName.Text = "Tên nhà *";
            this.lblName.Location = new Point(startX, startY);
            this.lblName.AutoSize = true;
            this.txtName.Location = new Point(startX, startY + 20);
            this.txtName.Size = new Size(400, 35);

            // Địa chỉ
            this.lblAddress.Text = "Địa chỉ *";
            this.lblAddress.Location = new Point(startX, startY + gapY);
            this.lblAddress.AutoSize = true;
            this.txtAddress.Location = new Point(startX, startY + gapY + 20);
            this.txtAddress.Size = new Size(400, 35);

            // Mô tả
            this.lblDescription.Text = "Mô tả";
            this.lblDescription.Location = new Point(startX, startY + gapY * 2);
            this.lblDescription.AutoSize = true;
            this.txtDescription.Location = new Point(startX, startY + gapY * 2 + 20);
            this.txtDescription.Size = new Size(400, 35);

            // Trạng thái
            this.lblStatus.Text = "Trạng thái *";
            this.lblStatus.Location = new Point(startX, startY + gapY * 3);
            this.lblStatus.AutoSize = true;
            this.cboStatus.Location = new Point(startX, startY + gapY * 3 + 20);
            this.cboStatus.Size = new Size(200, 35);
            this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });

            // Buttons
            this.btnSave.Text = "Lưu thông tin";
            this.btnSave.Location = new Point(170, startY + gapY * 4 + 20);
            this.btnSave.Size = new Size(120, 40);
            this.btnSave.BackColor = AppColors.Success;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Text = "Hủy bỏ";
            this.btnCancel.Location = new Point(310, startY + gapY * 4 + 20);
            this.btnCancel.Size = new Size(120, 40);
            this.btnCancel.BackColor = AppColors.Secondary;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Form
            this.ClientSize = new Size(480, 450);
            this.MinimumSize = new Size(480, 400);
            this.Controls.AddRange(new Control[] { lblTitle, lblName, txtName, lblAddress, txtAddress, lblDescription, txtDescription, lblStatus, cboStatus, btnSave, btnCancel });
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.AutoScroll = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppColors.Card;
            this.Text = "Quản lý Nhà";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}