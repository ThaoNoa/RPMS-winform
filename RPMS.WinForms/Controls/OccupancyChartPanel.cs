using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    /// <summary>Biểu đồ tròn tỷ lệ lấp đầy (Occupied / Total).</summary>
    public class OccupancyChartPanel : Panel
    {
        public string ChartTitle { get; set; } = "Tỷ lệ lấp đầy";
        public int Occupied { get; set; }
        public int Available { get; set; }
        public int Maintenance { get; set; }

        public OccupancyChartPanel()
        {
            DoubleBuffered = true;
            BackColor = AppColors.Card;
            Size = new Size(360, 220);
            Paint += OnPaintChart;
        }

        public void SetData(int occupied, int available, int maintenance = 0)
        {
            Occupied = Math.Max(0, occupied);
            Available = Math.Max(0, available);
            Maintenance = Math.Max(0, maintenance);
            Invalidate();
        }

        private void OnPaintChart(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            using var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var small = new Font("Segoe UI", 8.5F);
            using var titleBrush = new SolidBrush(AppColors.TextMain);
            using var muted = new SolidBrush(AppColors.TextMuted);
            g.DrawString(ChartTitle, titleFont, titleBrush, 12, 10);

            int total = Occupied + Available + Maintenance;
            double rate = total == 0 ? 0 : (double)Occupied / total * 100.0;

            var rect = new Rectangle(30, 48, 130, 130);
            if (total == 0)
            {
                using var pen = new Pen(AppColors.Border, 16);
                g.DrawEllipse(pen, rect);
            }
            else
            {
                float start = -90f;
                void Slice(int count, Color color)
                {
                    if (count <= 0) return;
                    float sweep = 360f * count / total;
                    using var brush = new SolidBrush(color);
                    g.FillPie(brush, rect, start, sweep);
                    start += sweep;
                }
                Slice(Occupied, AppColors.Success);
                Slice(Available, AppColors.Warning);
                Slice(Maintenance, AppColors.Danger);
                // hole
                using var hole = new SolidBrush(AppColors.Card);
                g.FillEllipse(hole, rect.X + 28, rect.Y + 28, rect.Width - 56, rect.Height - 56);
            }

            using var rateFont = new Font("Segoe UI", 14F, FontStyle.Bold);
            var rateText = $"{rate:0.#}%";
            var sz = g.MeasureString(rateText, rateFont);
            g.DrawString(rateText, rateFont, titleBrush,
                rect.X + (rect.Width - sz.Width) / 2,
                rect.Y + (rect.Height - sz.Height) / 2);

            int lx = 190, ly = 70;
            void Legend(Color c, string text)
            {
                using var b = new SolidBrush(c);
                g.FillRectangle(b, lx, ly, 12, 12);
                g.DrawString(text, small, muted, lx + 18, ly - 2);
                ly += 24;
            }
            Legend(AppColors.Success, $"Đã thuê: {Occupied}");
            Legend(AppColors.Warning, $"Trống: {Available}");
            if (Maintenance > 0)
                Legend(AppColors.Danger, $"Bảo trì: {Maintenance}");
            g.DrawString($"Tổng phòng: {total}", small, titleBrush, lx, ly + 4);
        }
    }
}
