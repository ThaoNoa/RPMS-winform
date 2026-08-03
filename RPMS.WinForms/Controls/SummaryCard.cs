using RPMS.Common.Constants;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class SummaryCard : Panel
    {
        private readonly Label lblTitle;
        private readonly Label lblValue;
        private Color _themeColor = AppColors.Primary;

        public SummaryCard()
        {
            Size = new Size(250, 120);
            BackColor = AppColors.Card;
            Padding = new Padding(16);
            Margin = new Padding(12);

            lblTitle = new Label
            {
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMuted,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblValue = new Label
            {
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblValue);
            Controls.Add(lblTitle);

            Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UI.UIHelper.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8);
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawPath(pen, path);
                using var brush = new SolidBrush(_themeColor);
                e.Graphics.FillRectangle(brush, 0, 8, 4, Height - 16);
            };
        }

        public string Title
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public string Value
        {
            get => lblValue.Text;
            set => lblValue.Text = value;
        }

        public Color ThemeColor
        {
            get => _themeColor;
            set { _themeColor = value; Invalidate(); }
        }
    }
}
