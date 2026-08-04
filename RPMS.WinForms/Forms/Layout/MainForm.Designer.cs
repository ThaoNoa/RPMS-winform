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

            // pnlTopbar — TableLayout: Title | User | Logout (không đè)
            this.pnlTopbar.BackColor = AppColors.Card;
            this.pnlTopbar.Dock = DockStyle.Top;
            this.pnlTopbar.Height = 64;
            this.pnlTopbar.Padding = new Padding(20, 10, 16, 10);
            this.pnlTopbar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppColors.Border, 1))
                    e.Graphics.DrawLine(p, 0, pnlTopbar.Height - 1, pnlTopbar.Width, pnlTopbar.Height - 1);
            };

            var topLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // lblPageTitle
            this.lblPageTitle.AutoSize = false;
            this.lblPageTitle.Dock = DockStyle.Fill;
            this.lblPageTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.lblPageTitle.ForeColor = AppColors.TextMain;
            this.lblPageTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblPageTitle.Text = "Dashboard";
            this.lblPageTitle.AutoEllipsis = true;

            // lblUserInfo
            this.lblUserInfo.AutoSize = false;
            this.lblUserInfo.Dock = DockStyle.Fill;
            this.lblUserInfo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblUserInfo.ForeColor = AppColors.TextMuted;
            this.lblUserInfo.TextAlign = ContentAlignment.MiddleRight;
            this.lblUserInfo.Padding = new Padding(8, 0, 8, 0);
            this.lblUserInfo.AutoEllipsis = true;
            this.lblUserInfo.MinimumSize = new Size(160, 0);
            this.lblUserInfo.MaximumSize = new Size(320, 0);

            // btnLogout
            this.btnLogout.Dock = DockStyle.Fill;
            this.btnLogout.Margin = new Padding(4, 0, 0, 0);
            this.btnLogout.Text = "Đăng xuất";
            this.btnLogout.BackColor = AppColors.Card;
            this.btnLogout.ForeColor = AppColors.Danger;
            this.btnLogout.BorderRadius = 6;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            topLayout.Controls.Add(this.lblPageTitle, 0, 0);
            topLayout.Controls.Add(this.lblUserInfo, 1, 0);
            topLayout.Controls.Add(this.btnLogout, 2, 0);
            this.pnlTopbar.Controls.Add(topLayout);

            // pnlContent
            this.pnlContent.BackColor = AppColors.Background;
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Padding = new Padding(20);

            // MainForm — cửa sổ chính có thể phóng to / kéo giãn
            this.ClientSize = new Size(1400, 850);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlTopbar);
            this.Controls.Add(this.pnlSidebar);
            this.MinimumSize = new Size(900, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.WindowState = FormWindowState.Maximized;
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