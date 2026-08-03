using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    /// <summary>Empty state thân thiện khi không có dữ liệu.</summary>
    public class EmptyStatePanel : Panel
    {
        private readonly Label _lblTitle;
        private readonly Label _lblHint;

        public EmptyStatePanel()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Visible = false;

            _lblTitle = new Label
            {
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Text = "Chưa có dữ liệu"
            };
            _lblHint = new Label
            {
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                Text = "Thử làm mới hoặc thay đổi bộ lọc."
            };
            Controls.Add(_lblTitle);
            Controls.Add(_lblHint);
            Resize += (s, e) => LayoutChildren();
        }

        public void ShowEmpty(string title, string hint)
        {
            _lblTitle.Text = title;
            _lblHint.Text = hint;
            Visible = true;
            BringToFront();
            LayoutChildren();
        }

        public void HideEmpty() => Visible = false;

        private void LayoutChildren()
        {
            _lblHint.MaximumSize = new Size(Math.Max(200, Width - 80), 0);
            _lblTitle.Location = new Point((Width - _lblTitle.PreferredWidth) / 2, Height / 2 - 36);
            _lblHint.Location = new Point((Width - Math.Min(_lblHint.PreferredWidth, _lblHint.MaximumSize.Width)) / 2, Height / 2);
        }
    }
}
