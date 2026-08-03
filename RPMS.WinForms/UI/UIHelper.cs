using RPMS.Common.Constants;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    public static class UIHelper
    {
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = AppColors.Background;
            form.Font = AppTypography.Body;
            form.ForeColor = AppColors.TextMain;
            if (form.FormBorderStyle == FormBorderStyle.FixedDialog ||
                form.FormBorderStyle == FormBorderStyle.FixedSingle ||
                form.FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                form.FormBorderStyle = FormBorderStyle.Sizable;
            }
            form.MaximizeBox = true;
            form.MinimizeBox = true;
            if (form.MinimumSize.Width < 480)
                form.MinimumSize = new Size(Math.Max(480, form.ClientSize.Width / 2), Math.Max(360, form.ClientSize.Height / 2));
        }

        /// <summary>Áp dụng cho form modal chi tiết — có thể resize.</summary>
        public static void ApplyResizableDialog(Form form, Size? minSize = null)
        {
            ApplyFormStyle(form);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = true;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimumSize = minSize ?? new Size(640, 520);
        }

        /// <summary>
        /// Chuẩn hóa trang danh sách: header Top + lưới Fill.
        /// </summary>
        public static void WireListPage(Form form, Control? header, Control content)
        {
            ApplyFormStyle(form);
            form.AutoScroll = false;
            if (header != null)
            {
                header.Dock = DockStyle.Top;
                if (header.Height < 48) header.Height = 56;
            }
            content.Dock = DockStyle.Fill;
            if (content is DataGridView dgv)
            {
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Gắn Anchor Right cho control nằm vùng phải; Left+Right cho input rộng.
        /// Gọi sau khi xây UI tuyệt đối (Designer) để giảm cắt khi resize.
        /// </summary>
        public static void SoftAnchorDialogControls(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                SoftAnchorDialogControls(c);
                if (c is TextBox or RichTextBox or ComboBox or ListBox or CheckedListBox)
                {
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
                else if (c is PictureBox pic)
                {
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                    pic.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                }
                else if (c is DataGridView dgv)
                {
                    dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                else if (c is Button or Panel)
                {
                    // Footer-ish buttons near bottom: keep Bottom anchor if already near bottom
                    if (parent is Form f && c.Top > f.ClientSize.Height * 0.7)
                        c.Anchor = AnchorStyles.Bottom | (c.Left > f.ClientSize.Width / 2 ? AnchorStyles.Right : AnchorStyles.Left);
                }
            }
        }

        public static void ApplyCardStyle(Panel panel)
        {
            panel.BackColor = AppColors.Card;
            panel.Padding = new Padding(16);
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawCardBorder(Graphics g, Rectangle bounds, int radius = 8)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(bounds, radius);
            using var pen = new Pen(AppColors.Border);
            g.DrawPath(pen, path);
        }

        public static Label CreateTitleLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = AppTypography.Subtitle,
                ForeColor = AppColors.TextMain,
                AutoSize = true
            };
        }

        public static Label CreateMutedLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMuted,
                AutoSize = true
            };
        }
    }
}
