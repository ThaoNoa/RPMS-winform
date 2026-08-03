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
            lblBrandDesc = new Label();
            lblBrandTagline = new Label();
            lblBrandName = new Label();
            pnlFormHost = new Panel();
            pnlCard = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblUsername = new Label();
            txtUsername = new ModernTextBox();
            lblPassword = new Label();
            txtPassword = new ModernTextBox();
            chkShowPassword = new CheckBox();
            lblErrorMessage = new Label();
            btnLogin = new ModernButton();
            lblRegisterHint = new Label();
            lblRegisterLink = new Label();
            lblDemoHint = new Label();
            pnlBrand.SuspendLayout();
            pnlFormHost.SuspendLayout();
            pnlCard.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBrand
            // 
            pnlBrand.BackColor = Color.FromArgb(15, 23, 42);
            pnlBrand.Controls.Add(lblBrandDesc);
            pnlBrand.Controls.Add(lblBrandTagline);
            pnlBrand.Controls.Add(lblBrandName);
            pnlBrand.Dock = DockStyle.Left;
            pnlBrand.Location = new Point(0, 0);
            pnlBrand.Name = "pnlBrand";
            pnlBrand.Size = new Size(380, 580);
            pnlBrand.TabIndex = 1;
            pnlBrand.Paint += PnlBrand_Paint;
            // 
            // lblBrandDesc
            // 
            lblBrandDesc.AutoSize = true;
            lblBrandDesc.Font = new Font("Segoe UI", 10F);
            lblBrandDesc.ForeColor = Color.FromArgb(148, 163, 184);
            lblBrandDesc.Location = new Point(52, 320);
            lblBrandDesc.MaximumSize = new Size(280, 0);
            lblBrandDesc.Name = "lblBrandDesc";
            lblBrandDesc.Size = new Size(268, 69);
            lblBrandDesc.TabIndex = 0;
            lblBrandDesc.Text = "Quản lý nhà trọ, hợp đồng, hóa đơn và bảo trì trên một nền tảng chuyên nghiệp.";
            // 
            // lblBrandTagline
            // 
            lblBrandTagline.AutoSize = true;
            lblBrandTagline.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblBrandTagline.ForeColor = Color.FromArgb(191, 219, 254);
            lblBrandTagline.Location = new Point(52, 230);
            lblBrandTagline.MaximumSize = new Size(280, 0);
            lblBrandTagline.Name = "lblBrandTagline";
            lblBrandTagline.Size = new Size(252, 64);
            lblBrandTagline.TabIndex = 1;
            lblBrandTagline.Text = "Rental Property\nManagement System";
            // 
            // lblBrandName
            // 
            lblBrandName.AutoSize = true;
            lblBrandName.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblBrandName.ForeColor = Color.White;
            lblBrandName.Location = new Point(48, 160);
            lblBrandName.Name = "lblBrandName";
            lblBrandName.Size = new Size(202, 81);
            lblBrandName.TabIndex = 2;
            lblBrandName.Text = "RPMS";
            // 
            // pnlFormHost
            // 
            pnlFormHost.BackColor = Color.FromArgb(248, 250, 252);
            pnlFormHost.Controls.Add(pnlCard);
            pnlFormHost.Dock = DockStyle.Fill;
            pnlFormHost.Location = new Point(380, 0);
            pnlFormHost.Name = "pnlFormHost";
            pnlFormHost.Size = new Size(540, 580);
            pnlFormHost.TabIndex = 0;
            // 
            // pnlCard
            // 
            pnlCard.BackColor = Color.FromArgb(255, 255, 255);
            pnlCard.Controls.Add(lblTitle);
            pnlCard.Controls.Add(lblSubtitle);
            pnlCard.Controls.Add(lblUsername);
            pnlCard.Controls.Add(txtUsername);
            pnlCard.Controls.Add(lblPassword);
            pnlCard.Controls.Add(txtPassword);
            pnlCard.Controls.Add(chkShowPassword);
            pnlCard.Controls.Add(lblErrorMessage);
            pnlCard.Controls.Add(btnLogin);
            pnlCard.Controls.Add(lblRegisterHint);
            pnlCard.Controls.Add(lblRegisterLink);
            pnlCard.Controls.Add(lblDemoHint);
            pnlCard.Location = new Point(0, 0);
            pnlCard.Name = "pnlCard";
            pnlCard.Padding = new Padding(36);
            pnlCard.Size = new Size(540, 580);
            pnlCard.TabIndex = 0;
            pnlCard.Paint += PnlCard_Paint;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Location = new Point(36, 36);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(211, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đăng nhập";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(107, 114, 128);
            lblSubtitle.Location = new Point(39, 95);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(273, 23);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Chào mừng trở lại hệ thống RPMS";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(17, 24, 39);
            lblUsername.Location = new Point(38, 130);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(128, 23);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Tên đăng nhập";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(255, 255, 255);
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.Location = new Point(38, 156);
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new Padding(12, 10, 12, 10);
            txtUsername.PlaceholderText = "ví dụ: admin";
            txtUsername.Size = new Size(340, 42);
            txtUsername.TabIndex = 3;
            txtUsername.UseSystemPasswordChar = false;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(17, 24, 39);
            lblPassword.Location = new Point(38, 216);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(86, 23);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Mật khẩu";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(255, 255, 255);
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Location = new Point(38, 242);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(12, 10, 12, 10);
            txtPassword.PlaceholderText = "••••••••";
            txtPassword.Size = new Size(340, 42);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPassword
            // 
            chkShowPassword.AutoSize = true;
            chkShowPassword.Font = new Font("Segoe UI", 9F);
            chkShowPassword.ForeColor = Color.FromArgb(107, 114, 128);
            chkShowPassword.Location = new Point(38, 296);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(127, 24);
            chkShowPassword.TabIndex = 6;
            chkShowPassword.Text = "Hiện mật khẩu";
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            // 
            // lblErrorMessage
            // 
            lblErrorMessage.Font = new Font("Segoe UI", 9F);
            lblErrorMessage.ForeColor = Color.FromArgb(239, 68, 68);
            lblErrorMessage.Location = new Point(38, 324);
            lblErrorMessage.Name = "lblErrorMessage";
            lblErrorMessage.Size = new Size(340, 36);
            lblErrorMessage.TabIndex = 7;
            lblErrorMessage.Visible = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.BorderRadius = 8;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(38, 362);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(340, 46);
            btnLogin.TabIndex = 8;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // lblRegisterHint
            // 
            lblRegisterHint.AutoSize = true;
            lblRegisterHint.Font = new Font("Segoe UI", 10F);
            lblRegisterHint.ForeColor = Color.FromArgb(107, 114, 128);
            lblRegisterHint.Location = new Point(38, 424);
            lblRegisterHint.Name = "lblRegisterHint";
            lblRegisterHint.Size = new Size(157, 23);
            lblRegisterHint.TabIndex = 9;
            lblRegisterHint.Text = "Chưa có tài khoản?";
            // 
            // lblRegisterLink
            // 
            lblRegisterLink.AutoSize = true;
            lblRegisterLink.Cursor = Cursors.Hand;
            lblRegisterLink.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRegisterLink.ForeColor = Color.FromArgb(37, 99, 235);
            lblRegisterLink.Location = new Point(201, 424);
            lblRegisterLink.Name = "lblRegisterLink";
            lblRegisterLink.Size = new Size(121, 23);
            lblRegisterLink.TabIndex = 10;
            lblRegisterLink.Text = "Đăng ký ngay";
            lblRegisterLink.Click += lblRegisterLink_Click;
            // 
            // lblDemoHint
            // 
            lblDemoHint.AutoSize = true;
            lblDemoHint.Font = new Font("Segoe UI", 9F);
            lblDemoHint.ForeColor = Color.FromArgb(107, 114, 128);
            lblDemoHint.Location = new Point(38, 452);
            lblDemoHint.MaximumSize = new Size(340, 0);
            lblDemoHint.Name = "lblDemoHint";
            lblDemoHint.Size = new Size(326, 40);
            lblDemoHint.TabIndex = 11;
            lblDemoHint.Text = "Demo: admin/admin123 | namlandlord | tenant | manager — MK: 123456";
            // 
            // LoginForm
            // 
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(920, 580);
            Controls.Add(pnlFormHost);
            Controls.Add(pnlBrand);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RPMS — Đăng nhập";
            Load += LoginForm_Load;
            Resize += LoginForm_Resize;
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

        private void PnlCard_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = UI.UIHelper.RoundedRect(new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1), 12))
            {
                using (var pen = new Pen(AppColors.Border))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void PnlBrand_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(
                pnlBrand.ClientRectangle,
                AppColors.Sidebar,
                Color.FromArgb(30, 58, 138),
                45f))
            {
                g.FillRectangle(brush, pnlBrand.ClientRectangle);
            }

            using (var accent = new SolidBrush(Color.FromArgb(60, AppColors.Primary)))
            {
                g.FillEllipse(accent, pnlBrand.Width - 160, -40, 220, 220);
                g.FillEllipse(accent, -80, pnlBrand.Height - 160, 240, 240);
            }
        }

        private void ChkShowPassword_CheckedChanged(object? sender, System.EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void LoginForm_Load(object? sender, System.EventArgs e)
        {
            CenterCard();
        }

        private void LoginForm_Resize(object? sender, System.EventArgs e)
        {
            CenterCard();
        }
    }
}