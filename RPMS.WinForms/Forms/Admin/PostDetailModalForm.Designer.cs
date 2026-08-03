using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    partial class PostDetailModalForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblHeader;
        private RichTextBox rtxtContent;
        private ModernButton btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblHeader = new Label();
            this.rtxtContent = new RichTextBox();
            this.btnClose = new ModernButton();
            this.SuspendLayout();

            // lblHeader
            this.lblHeader.Text = "Chi tiết Tin Đăng";
            this.lblHeader.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblHeader.ForeColor = AppColors.Primary;
            this.lblHeader.Location = new Point(20, 20);
            this.lblHeader.AutoSize = true;

            // rtxtContent
            this.rtxtContent.Location = new Point(20, 60);
            this.rtxtContent.Size = new Size(540, 300);
            this.rtxtContent.ReadOnly = true;
            this.rtxtContent.BackColor = AppColors.Background;
            this.rtxtContent.BorderStyle = BorderStyle.None;
            this.rtxtContent.Font = new Font("Segoe UI", 10F);

            // btnClose
            this.btnClose.Text = "Đóng";
            this.btnClose.Location = new Point(230, 380);
            this.btnClose.Size = new Size(120, 40);
            this.btnClose.BackColor = AppColors.Secondary;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // Form
            this.ClientSize = new Size(580, 450);
            this.MinimumSize = new Size(480, 400);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.rtxtContent);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.AutoScroll = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppColors.Card;
            this.Text = "Chi tiết Tin Đăng";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}