using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class ModernButton : Button
    {
        private int _borderRadius = 8;
        private bool _isHovered;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Size = new Size(150, 42);
            BackColor = AppColors.Primary;
            ForeColor = Color.White;
            Font = AppTypography.Button;
            Cursor = Cursors.Hand;
            ResizeRedraw = true;
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                UpdateRegion();
                Invalidate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(Parent?.BackColor ?? AppColors.Card);

            var rect = ClientRectangle;
            using var path = Rounded(rect, _borderRadius);

            Color fill = BackColor;
            if (!Enabled)
                fill = Color.FromArgb(180, AppColors.Border);
            else if (_isHovered)
                fill = BackColor.ToArgb() == AppColors.Primary.ToArgb()
                    ? AppColors.PrimaryHover
                    : ControlPaint.Dark(BackColor, 0.05f);

            using (var brush = new SolidBrush(fill))
                pevent.Graphics.FillPath(brush, path);

            // Không tạo Region trong Paint (tránh leak GDI → crash Font)
            TextRenderer.DrawText(
                pevent.Graphics,
                Text,
                Font,
                rect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void UpdateRegion()
        {
            if (!IsHandleCreated || Width <= 0 || Height <= 0) return;
            using var path = Rounded(ClientRectangle, _borderRadius);
            var old = Region;
            Region = new Region(path);
            old?.Dispose();
        }

        private static GraphicsPath Rounded(Rectangle bounds, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
