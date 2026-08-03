using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Auth
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlBrand;
        private Panel pnlFormHost;
        private Panel pnlCard;
        private Label lblBrandName;
        private Label lblBrandTagline;
        private Label lblBrandDesc;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblUsername;
        private ModernTextBox txtUsername;
        private Label lblPassword;
        private ModernTextBox txtPassword;
        private CheckBox chkShowPassword;
        private ModernButton btnLogin;
        private Label lblErrorMessage;
        private Label lblRegisterHint;
        private Label lblRegisterLink;
        private Label lblDemoHint;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlBrand = new Panel();
            lblBrandName = new Label();
            lblBrandTagline = new Label();
            lblBrandDesc = new Label();
            pnlFormHost = new Panel();
            pnlCard = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblUsername = new Label();
            txtUsername = new ModernTextBox();
            lblPassword = new Label();
            txtPassword = new ModernTextBox();
            chkShowPassword = new CheckBox();
            btnLogin = new ModernButton();
            lblErrorMessage = new Label();
            lblRegisterHint = new Label();
            lblRegisterLink = new Label();
            lblDemoHint = new Label();

            SuspendLayout();
            pnlBrand.SuspendLayout();
            pnlFormHost.SuspendLayout();
            pnlCard.SuspendLayout();

            // Brand panel (Fluent dark)
            pnlBrand.Dock = DockStyle.Left;
            pnlBrand.Width = 380;
            pnlBrand.BackColor = AppColors.Sidebar;
            pnlBrand.Paint += PnlBrand_Paint;
            pnlBrand.Controls.Add(lblBrandDesc);
            pnlBrand.Controls.Add(lblBrandTagline);
            pnlBrand.Controls.Add(lblBrandName);

            lblBrandName.AutoSize = true;
            lblBrandName.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblBrandName.ForeColor = Color.White;
            lblBrandName.Location = new Point(48, 160);
            lblBrandName.Text = "RPMS";

            lblBrandTagline.AutoSize = true;
            lblBrandTagline.Font = AppTypography.Heading;
            lblBrandTagline.ForeColor = Color.FromArgb(191, 219, 254);
            lblBrandTagline.Location = new Point(52, 230);
            lblBrandTagline.MaximumSize = new Size(280, 0);
            lblBrandTagline.Text = "Rental Property\nManagement System";

            lblBrandDesc.AutoSize = true;
            lblBrandDesc.Font = AppTypography.Body;
            lblBrandDesc.ForeColor = Color.FromArgb(148, 163, 184);
            lblBrandDesc.Location = new Point(52, 320);
            lblBrandDesc.MaximumSize = new Size(280, 0);
            lblBrandDesc.Text = "Quản lý nhà trọ, hợp đồng, hóa đơn và bảo trì trên một nền tảng chuyên nghiệp.";

            // Form host
            pnlFormHost.Dock = DockStyle.Fill;
            pnlFormHost.BackColor = AppColors.Background;
            pnlFormHost.Controls.Add(pnlCard);

            // Card
            pnlCard.Size = new Size(420, 480);
            pnlCard.BackColor = AppColors.Card;
            pnlCard.Padding = new Padding(36);
            pnlCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UI.UIHelper.RoundedRect(new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1), 12);
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawPath(pen, path);
            };

            lblTitle.AutoSize = true;
            lblTitle.Font = AppTypography.Title;
            lblTitle.ForeColor = AppColors.TextMain;
            lblTitle.Location = new Point(36, 36);
            lblTitle.Text = "Đăng nhập";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = AppTypography.Body;
            lblSubtitle.ForeColor = AppColors.TextMuted;
            lblSubtitle.Location = new Point(38, 78);
            lblSubtitle.Text = "Chào mừng trở lại hệ thống RPMS";

            lblUsername.AutoSize = true;
            lblUsername.Font = AppTypography.BodyBold;
            lblUsername.ForeColor = AppColors.TextMain;
            lblUsername.Location = new Point(38, 130);
            lblUsername.Text = "Tên đăng nhập";

            txtUsername.Location = new Point(38, 156);
            txtUsername.Size = new Size(340, 42);
            txtUsername.PlaceholderText = "ví dụ: admin";

            lblPassword.AutoSize = true;
            lblPassword.Font = AppTypography.BodyBold;
            lblPassword.ForeColor = AppColors.TextMain;
            lblPassword.Location = new Point(38, 216);
            lblPassword.Text = "Mật khẩu";

            txtPassword.Location = new Point(38, 242);
            txtPassword.Size = new Size(340, 42);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.PlaceholderText = "••••••••";

            chkShowPassword.AutoSize = true;
            chkShowPassword.Font = AppTypography.Caption;
            chkShowPassword.ForeColor = AppColors.TextMuted;
            chkShowPassword.Location = new Point(38, 296);
            chkShowPassword.Text = "Hiện mật khẩu";
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lblErrorMessage.AutoSize = false;
            lblErrorMessage.Size = new Size(340, 36);
            lblErrorMessage.Font = AppTypography.Caption;
            lblErrorMessage.ForeColor = AppColors.Danger;
            lblErrorMessage.Location = new Point(38, 324);
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;

            btnLogin.Location = new Point(38, 362);
            btnLogin.Size = new Size(340, 46);
            btnLogin.Text = "Đăng nhập";
            btnLogin.BackColor = AppColors.Primary;
            btnLogin.Click += btnLogin_Click;

            lblRegisterHint.AutoSize = true;
            lblRegisterHint.Font = AppTypography.Body;
            lblRegisterHint.ForeColor = AppColors.TextMuted;
            lblRegisterHint.Location = new Point(38, 424);
            lblRegisterHint.Text = "Chưa có tài khoản?";

            lblRegisterLink.AutoSize = true;
            lblRegisterLink.Font = AppTypography.BodyBold;
            lblRegisterLink.ForeColor = AppColors.Primary;
            lblRegisterLink.Location = new Point(170, 424);
            lblRegisterLink.Text = "Đăng ký ngay";
            lblRegisterLink.Cursor = Cursors.Hand;
            lblRegisterLink.Click += lblRegisterLink_Click;

            lblDemoHint.AutoSize = true;
            lblDemoHint.Font = AppTypography.Caption;
            lblDemoHint.ForeColor = AppColors.TextMuted;
            lblDemoHint.Location = new Point(38, 452);
            lblDemoHint.MaximumSize = new Size(340, 0);
            lblDemoHint.Text = "Demo: admin / landlord1 / tenant1 / manager1 — MK: 123456";

            pnlCard.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubtitle, lblUsername, txtUsername, lblPassword, txtPassword,
                chkShowPassword, lblErrorMessage, btnLogin, lblRegisterHint, lblRegisterLink, lblDemoHint
            });

            pnlFormHost.Controls.Add(pnlCard);
            Controls.Add(pnlFormHost);
            Controls.Add(pnlBrand);

            ClientSize = new Size(920, 580);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RPMS — Đăng nhập";
            BackColor = AppColors.Background;
            Font = AppTypography.Body;

            Load += (s, e) => CenterCard();
            Resize += (s, e) => CenterCard();

            pnlBrand.ResumeLayout(false);
            pnlBrand.PerformLayout();
            pnlFormHost.ResumeLayout(false);
            pnlCard.ResumeLayout(false);
            pnlCard.PerformLayout();
            ResumeLayout(false);
        }

        private void CenterCard()
        {
            if (pnlCard == null || pnlFormHost == null) return;
            pnlCard.Left = Math.Max(24, (pnlFormHost.Width - pnlCard.Width) / 2);
            pnlCard.Top = Math.Max(24, (pnlFormHost.Height - pnlCard.Height) / 2);
        }

        private void PnlBrand_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(
                pnlBrand.ClientRectangle,
                AppColors.Sidebar,
                Color.FromArgb(30, 58, 138),
                45f);
            g.FillRectangle(brush, pnlBrand.ClientRectangle);

            using var accent = new SolidBrush(Color.FromArgb(60, AppColors.Primary));
            g.FillEllipse(accent, pnlBrand.Width - 160, -40, 220, 220);
            g.FillEllipse(accent, -80, pnlBrand.Height - 160, 240, 240);
        }
    }
}
