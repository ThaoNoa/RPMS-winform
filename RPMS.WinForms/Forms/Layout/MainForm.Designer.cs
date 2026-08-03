using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Layout
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlSidebar;
        private Panel pnlLogo;
        private Label lblLogo;
        private FlowLayoutPanel flpMenu;
        private Panel pnlTopbar;
        private Label lblPageTitle;
        private Label lblUserInfo;
        private ModernButton btnLogout;
        private Panel pnlContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new Panel();
            this.pnlLogo = new Panel();
            this.lblLogo = new Label();
            this.flpMenu = new FlowLayoutPanel();
            this.pnlTopbar = new Panel();
            this.lblPageTitle = new Label();
            this.lblUserInfo = new Label();
            this.btnLogout = new ModernButton();
            this.pnlContent = new Panel();

            this.pnlSidebar.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            this.pnlTopbar.SuspendLayout();
            this.SuspendLayout();

            // pnlSidebar
            this.pnlSidebar.BackColor = AppColors.Sidebar;
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Width = 250;
            this.pnlSidebar.Controls.Add(this.flpMenu);
            this.pnlSidebar.Controls.Add(this.pnlLogo);

            // pnlLogo
            this.pnlLogo.Dock = DockStyle.Top;
            this.pnlLogo.Height = 70;
            this.pnlLogo.BackColor = AppColors.Sidebar;
            this.pnlLogo.Controls.Add(this.lblLogo);

            // lblLogo
            this.lblLogo.Dock = DockStyle.Fill;
            this.lblLogo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblLogo.ForeColor = Color.White;
            this.lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            this.lblLogo.Text = "RPMS";

            // flpMenu
            this.flpMenu.Dock = DockStyle.Fill;
            this.flpMenu.FlowDirection = FlowDirection.TopDown;
            this.flpMenu.WrapContents = false;
            this.flpMenu.AutoScroll = true;
            this.flpMenu.BackColor = AppColors.Sidebar;
            this.flpMenu.Padding = new Padding(0, 10, 0, 10);

            // pnlTopbar
            this.pnlTopbar.BackColor = AppColors.Card;
            this.pnlTopbar.Dock = DockStyle.Top;
            this.pnlTopbar.Height = 70;
            this.pnlTopbar.Controls.Add(this.lblPageTitle);
            this.pnlTopbar.Controls.Add(this.lblUserInfo);
            this.pnlTopbar.Controls.Add(this.btnLogout);
            this.pnlTopbar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppColors.Border, 1))
                {
                    e.Graphics.DrawLine(p, 0, pnlTopbar.Height - 1, pnlTopbar.Width, pnlTopbar.Height - 1);
                }
            };

            // lblPageTitle
            this.lblPageTitle.AutoSize = true;
            this.lblPageTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblPageTitle.ForeColor = AppColors.TextMain;
            this.lblPageTitle.Location = new Point(30, 20);
            this.lblPageTitle.Text = "Dashboard";

            // btnLogout
            this.btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnLogout.Location = new Point(880, 15);
            this.btnLogout.Size = new Size(100, 40);
            this.btnLogout.Text = "Đăng xuất";
            this.btnLogout.BackColor = AppColors.Card;
            this.btnLogout.ForeColor = AppColors.Danger;
            this.btnLogout.BorderRadius = 4;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // lblUserInfo
            this.lblUserInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.lblUserInfo.AutoSize = true;
            this.lblUserInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblUserInfo.ForeColor = AppColors.TextMain;
            this.lblUserInfo.Location = new Point(650, 25);
            this.lblUserInfo.TextAlign = ContentAlignment.MiddleRight;
            this.lblUserInfo.RightToLeft = RightToLeft.Yes;

            // pnlContent
            this.pnlContent.BackColor = AppColors.Background;
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Padding = new Padding(20);

            // MainForm
            this.ClientSize = new Size(1280, 720);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlTopbar);
            this.Controls.Add(this.pnlSidebar);
            this.MinimumSize = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "RPMS - Rental Property Management System";
            this.BackColor = AppColors.Background;

            this.pnlLogo.ResumeLayout(false);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlTopbar.ResumeLayout(false);
            this.pnlTopbar.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}