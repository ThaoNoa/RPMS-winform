using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    public class AppDialog : Form
    {
        private readonly Label _lblMessage;
        private readonly ModernButton _btnOk;
        private readonly ModernButton? _btnCancel;
        private DialogResult _result = DialogResult.None;

        private AppDialog(string title, string message, bool showCancel, MessageBoxIcon icon)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = AppColors.Card;
            Font = AppTypography.Body;
            ClientSize = new Size(440, showCancel ? 200 : 180);
            Padding = new Padding(24);

            var accent = icon switch
            {
                MessageBoxIcon.Error => AppColors.Danger,
                MessageBoxIcon.Warning => AppColors.Warning,
                MessageBoxIcon.Information => AppColors.Primary,
                MessageBoxIcon.Question => AppColors.Primary,
                _ => AppColors.Primary
            };

            var accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accent
            };

            _lblMessage = new Label
            {
                Text = message,
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMain,
                Location = new Point(24, 30),
                Size = new Size(390, 80),
                AutoSize = false
            };

            _btnOk = new ModernButton
            {
                Text = "Đồng ý",
                Size = new Size(110, 36),
                Location = new Point(showCancel ? 200 : 310, 125),
                BackColor = AppColors.Primary
            };
            _btnOk.Click += (s, e) =>
            {
                _result = DialogResult.OK;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(accentBar);
            Controls.Add(_lblMessage);
            Controls.Add(_btnOk);

            if (showCancel)
            {
                _btnCancel = new ModernButton
                {
                    Text = "Hủy",
                    Size = new Size(110, 36),
                    Location = new Point(320, 125),
                    BackColor = AppColors.TextMuted
                };
                _btnCancel.Click += (s, e) =>
                {
                    _result = DialogResult.Cancel;
                    DialogResult = DialogResult.Cancel;
                    Close();
                };
                Controls.Add(_btnCancel);
                AcceptButton = _btnOk;
                CancelButton = _btnCancel;
            }
            else
            {
                AcceptButton = _btnOk;
            }
        }

        public static void ShowInfo(string message, string title = "Thông báo")
        {
            using var dlg = new AppDialog(title, message, false, MessageBoxIcon.Information);
            dlg.ShowDialog();
        }

        public static void ShowWarning(string message, string title = "Cảnh báo")
        {
            using var dlg = new AppDialog(title, message, false, MessageBoxIcon.Warning);
            dlg.ShowDialog();
        }

        public static void ShowError(string message, string title = "Lỗi")
        {
            using var dlg = new AppDialog(title, message, false, MessageBoxIcon.Error);
            dlg.ShowDialog();
        }

        public static bool Confirm(string message, string title = "Xác nhận")
        {
            using var dlg = new AppDialog(title, message, true, MessageBoxIcon.Question);
            return dlg.ShowDialog() == DialogResult.OK;
        }

        public static string? Prompt(string message, string title = "Nhập thông tin", string defaultValue = "")
        {
            using var form = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = AppColors.Card,
                ClientSize = new Size(440, 200),
                Font = AppTypography.Body
            };

            var lbl = new Label
            {
                Text = message,
                Location = new Point(24, 20),
                Size = new Size(390, 40),
                ForeColor = AppColors.TextMain
            };
            var txt = new TextBox
            {
                Text = defaultValue,
                Location = new Point(24, 70),
                Size = new Size(390, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            var btnOk = new ModernButton
            {
                Text = "Đồng ý",
                Size = new Size(110, 36),
                Location = new Point(190, 130),
                BackColor = AppColors.Primary,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new ModernButton
            {
                Text = "Hủy",
                Size = new Size(110, 36),
                Location = new Point(310, 130),
                BackColor = AppColors.TextMuted,
                DialogResult = DialogResult.Cancel
            };

            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }
    }
}
