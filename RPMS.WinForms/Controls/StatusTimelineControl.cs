using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    /// <summary>Timeline 3 mốc: Pending → Processing → Completed.</summary>
    public class StatusTimelineControl : Panel
    {
        private string _status = "Pending";

        public StatusTimelineControl()
        {
            Height = 72;
            Dock = DockStyle.Top;
            BackColor = AppColors.Card;
            DoubleBuffered = true;
            Paint += OnPaintTimeline;
        }

        public void SetStatus(string status)
        {
            _status = status ?? "Pending";
            Invalidate();
        }

        private int StepIndex => _status switch
        {
            "Processing" => 1,
            "Completed" => 2,
            _ => 0
        };

        private void OnPaintTimeline(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            string[] labels = { "Chờ xử lý", "Đang xử lý", "Hoàn thành" };
            int n = labels.Length;
            int pad = 40;
            int y = 28;
            int usable = Math.Max(100, Width - pad * 2);
            int step = usable / (n - 1);
            int active = StepIndex;

            using var linePen = new Pen(AppColors.Border, 3);
            using var activePen = new Pen(AppColors.Primary, 3);
            g.DrawLine(linePen, pad, y, pad + usable, y);
            if (active > 0)
                g.DrawLine(activePen, pad, y, pad + step * active, y);

            using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            using var muted = new SolidBrush(AppColors.TextMuted);
            using var text = new SolidBrush(AppColors.TextMain);

            for (int i = 0; i < n; i++)
            {
                int x = pad + i * step;
                bool done = i <= active;
                Color fill = done
                    ? (i == 2 && active == 2 ? AppColors.Success : AppColors.Primary)
                    : Color.White;
                using var brush = new SolidBrush(fill);
                using var pen = new Pen(done ? fill : AppColors.Border, 2);
                g.FillEllipse(brush, x - 9, y - 9, 18, 18);
                g.DrawEllipse(pen, x - 9, y - 9, 18, 18);
                if (done)
                {
                    using var check = new Pen(Color.White, 2);
                    g.DrawLines(check, new[] { new Point(x - 4, y), new Point(x - 1, y + 3), new Point(x + 5, y - 4) });
                }
                var sz = g.MeasureString(labels[i], font);
                g.DrawString(labels[i], font, done ? text : muted, x - sz.Width / 2, y + 14);
            }
        }
    }
}
