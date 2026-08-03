using RPMS.Common.Constants;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    /// <summary>Overlay loading bán trong suốt phủ lên form/panel.</summary>
    public class LoadingPanel : Panel
    {
        private readonly Label _lbl;
        private readonly ProgressBar _bar;

        public LoadingPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(180, 248, 250, 252);
            Visible = false;
            _lbl = new Label
            {
                Text = "Đang tải dữ liệu…",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoSize = true
            };
            _bar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size = new Size(220, 8)
            };
            Controls.Add(_lbl);
            Controls.Add(_bar);
            Resize += (s, e) => CenterChildren();
        }

        public void ShowLoading(string? text = null)
        {
            if (!string.IsNullOrWhiteSpace(text)) _lbl.Text = text;
            Visible = true;
            BringToFront();
            CenterChildren();
        }

        public void HideLoading() => Visible = false;

        private void CenterChildren()
        {
            _lbl.Location = new Point((Width - _lbl.Width) / 2, Height / 2 - 28);
            _bar.Location = new Point((Width - _bar.Width) / 2, Height / 2 + 4);
        }
    }
}
