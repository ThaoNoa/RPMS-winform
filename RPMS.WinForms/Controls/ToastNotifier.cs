using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    /// <summary>Thông báo toast góc dưới-phải, tự ẩn.</summary>
    public static class ToastNotifier
    {
        public static void Show(Form? owner, string message, ToastKind kind = ToastKind.Info, int ms = 2800)
        {
            if (owner == null || owner.IsDisposed) return;
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                BackColor = kind switch
                {
                    ToastKind.Success => Color.FromArgb(22, 163, 74),
                    ToastKind.Warning => Color.FromArgb(217, 119, 6),
                    ToastKind.Error => Color.FromArgb(220, 38, 38),
                    _ => AppColors.Primary
                },
                Opacity = 0.96,
                Size = new Size(320, 56)
            };
            var lbl = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 14, 0)
            };
            toast.Controls.Add(lbl);
            var screen = owner.RectangleToScreen(owner.ClientRectangle);
            toast.Location = new Point(screen.Right - toast.Width - 24, screen.Bottom - toast.Height - 24);
            toast.Show(owner);
            var t = new System.Windows.Forms.Timer { Interval = ms };
            t.Tick += (s, e) =>
            {
                t.Stop();
                t.Dispose();
                try { toast.Close(); toast.Dispose(); } catch { /* ignore */ }
            };
            t.Start();
        }
    }

    public enum ToastKind { Info, Success, Warning, Error }
}
