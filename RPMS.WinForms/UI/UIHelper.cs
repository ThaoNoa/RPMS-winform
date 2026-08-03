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
