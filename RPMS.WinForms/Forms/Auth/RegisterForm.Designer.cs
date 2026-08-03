using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Auth
{
    partial class RegisterForm
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
        private Label lblConfirmPassword;
        private ModernTextBox txtConfirmPassword;
        private Label lblFullName;
        private ModernTextBox txtFullName;
        private Label lblEmail;
        private ModernTextBox txtEmail;
        private Label lblPhone;
        private ModernTextBox txtPhone;
        private Label lblAddress;
        private ModernTextBox txtAddress;
        private Label lblRole;
        private ComboBox cboRole;
        private ModernButton btnRegister;
        private Label lblErrorMessage;
        private Label lblLoginHint;
        private Label lblLoginLink;

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
            lblConfirmPassword = new Label();
            txtConfirmPassword = new ModernTextBox();
            lblFullName = new Label();
            txtFullName = new ModernTextBox();
            lblEmail = new Label();
            txtEmail = new ModernTextBox();
            lblPhone = new Label();
            txtPhone = new ModernTextBox();
            lblAddress = new Label();
            txtAddress = new ModernTextBox();
            lblRole = new Label();
            cboRole = new ComboBox();
            btnRegister = new ModernButton();
            lblErrorMessage = new Label();
            lblLoginHint = new Label();
            lblLoginLink = new Label();

            SuspendLayout();
            pnlBrand.SuspendLayout();
            pnlFormHost.SuspendLayout();
            pnlCard.SuspendLayout();

            pnlBrand.Dock = DockStyle.Left;
            pnlBrand.Width = 340;
            pnlBrand.BackColor = AppColors.Sidebar;
            pnlBrand.Paint += PnlBrand_Paint;
            pnlBrand.Controls.Add(lblBrandDesc);
            pnlBrand.Controls.Add(lblBrandTagline);
            pnlBrand.Controls.Add(lblBrandName);

            lblBrandName.AutoSize = true;
            lblBrandName.Font = new Font("Segoe UI", 32F, FontStyle.Bold);
            lblBrandName.ForeColor = Color.White;
            lblBrandName.Location = new Point(40, 180);
            lblBrandName.Text = "RPMS";

            lblBrandTagline.AutoSize = true;
            lblBrandTagline.Font = AppTypography.Heading;
            lblBrandTagline.ForeColor = Color.FromArgb(191, 219, 254);
            lblBrandTagline.Location = new Point(42, 245);
            lblBrandTagline.MaximumSize = new Size(250, 0);
            lblBrandTagline.Text = "Tạo tài khoản mới";

            lblBrandDesc.AutoSize = true;
            lblBrandDesc.Font = AppTypography.Body;
            lblBrandDesc.ForeColor = Color.FromArgb(148, 163, 184);
            lblBrandDesc.Location = new Point(42, 300);
            lblBrandDesc.MaximumSize = new Size(250, 0);
            lblBrandDesc.Text = "Đăng ký với vai trò Landlord, Tenant hoặc Manager để bắt đầu sử dụng.";

            pnlFormHost.Dock = DockStyle.Fill;
            pnlFormHost.BackColor = AppColors.Background;
            pnlFormHost.AutoScroll = true;

            pnlCard.Size = new Size(620, 620);
            pnlCard.BackColor = AppColors.Card;
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
            lblTitle.Location = new Point(32, 28);
            lblTitle.Text = "Đăng ký";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = AppTypography.Body;
            lblSubtitle.ForeColor = AppColors.TextMuted;
            lblSubtitle.Location = new Point(34, 70);
            lblSubtitle.Text = "Điền thông tin bên dưới để tạo tài khoản";

            // Left column
            PlaceField(lblUsername, txtUsername, "Tên đăng nhập *", "username", 34, 110, 260);
            PlaceField(lblPassword, txtPassword, "Mật khẩu *", "ít nhất 6 ký tự", 34, 190, 260, true);
            PlaceField(lblConfirmPassword, txtConfirmPassword, "Xác nhận mật khẩu *", "nhập lại mật khẩu", 34, 270, 260, true);
            PlaceField(lblFullName, txtFullName, "Họ và tên *", "Nguyễn Văn A", 34, 350, 260);

            // Right column
            PlaceField(lblEmail, txtEmail, "Email *", "email@domain.com", 320, 110, 260);
            PlaceField(lblPhone, txtPhone, "Số điện thoại", "09xxxxxxxx", 320, 190, 260);
            PlaceField(lblAddress, txtAddress, "Địa chỉ", "Quận / Thành phố", 320, 270, 260);

            lblRole.AutoSize = true;
            lblRole.Font = AppTypography.BodyBold;
            lblRole.ForeColor = AppColors.TextMain;
            lblRole.Location = new Point(320, 350);
            lblRole.Text = "Vai trò *";

            cboRole.Location = new Point(320, 376);
            cboRole.Size = new Size(260, 36);
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.FlatStyle = FlatStyle.Flat;
            cboRole.Font = AppTypography.Body;
            cboRole.BackColor = AppColors.Card;

            lblErrorMessage.AutoSize = false;
            lblErrorMessage.Size = new Size(546, 36);
            lblErrorMessage.Font = AppTypography.Caption;
            lblErrorMessage.ForeColor = AppColors.Danger;
            lblErrorMessage.Location = new Point(34, 450);
            lblErrorMessage.Visible = false;

            btnRegister.Location = new Point(34, 495);
            btnRegister.Size = new Size(546, 46);
            btnRegister.Text = "Tạo tài khoản";
            btnRegister.BackColor = AppColors.Primary;
            btnRegister.Click += btnRegister_Click;

            lblLoginHint.AutoSize = true;
            lblLoginHint.Font = AppTypography.Body;
            lblLoginHint.ForeColor = AppColors.TextMuted;
            lblLoginHint.Location = new Point(34, 560);
            lblLoginHint.Text = "Đã có tài khoản?";

            lblLoginLink.AutoSize = true;
            lblLoginLink.Font = AppTypography.BodyBold;
            lblLoginLink.ForeColor = AppColors.Primary;
            lblLoginLink.Location = new Point(160, 560);
            lblLoginLink.Text = "Đăng nhập";
            lblLoginLink.Cursor = Cursors.Hand;
            lblLoginLink.Click += lblLoginLink_Click;

            pnlCard.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubtitle,
                lblUsername, txtUsername, lblPassword, txtPassword, lblConfirmPassword, txtConfirmPassword, lblFullName, txtFullName,
                lblEmail, txtEmail, lblPhone, txtPhone, lblAddress, txtAddress, lblRole, cboRole,
                lblErrorMessage, btnRegister, lblLoginHint, lblLoginLink
            });

            pnlFormHost.Controls.Add(pnlCard);
            Controls.Add(pnlFormHost);
            Controls.Add(pnlBrand);

            ClientSize = new Size(1020, 700);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "RPMS — Đăng ký";
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

        private void PlaceField(Label label, ModernTextBox box, string labelText, string placeholder, int x, int y, int width, bool password = false)
        {
            label.AutoSize = true;
            label.Font = AppTypography.BodyBold;
            label.ForeColor = AppColors.TextMain;
            label.Location = new Point(x, y);
            label.Text = labelText;

            box.Location = new Point(x, y + 26);
            box.Size = new Size(width, 40);
            box.PlaceholderText = placeholder;
            box.UseSystemPasswordChar = password;
        }

        private void CenterCard()
        {
            if (pnlCard == null || pnlFormHost == null) return;
            pnlCard.Left = Math.Max(20, (pnlFormHost.ClientSize.Width - pnlCard.Width) / 2);
            pnlCard.Top = Math.Max(20, (pnlFormHost.ClientSize.Height - pnlCard.Height) / 2);
        }

        private void PnlBrand_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(
                pnlBrand.ClientRectangle,
                AppColors.Sidebar,
                Color.FromArgb(30, 64, 175),
                50f);
            g.FillRectangle(brush, pnlBrand.ClientRectangle);
            using var accent = new SolidBrush(Color.FromArgb(50, AppColors.Primary));
            g.FillEllipse(accent, pnlBrand.Width - 140, -30, 200, 200);
            g.FillEllipse(accent, -60, pnlBrand.Height - 140, 200, 200);
        }
    }
}
