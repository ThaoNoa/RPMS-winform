using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    /// <summary>
    /// Chuẩn hóa đường dẫn ảnh (/uploads/...) và tạo placeholder khi file thiếu.
    /// </summary>
    public static class ImagePathHelper
    {
        /// <summary>True nếu đường dẫn là video.</summary>
        public static bool IsVideo(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".mp4" or ".webm" or ".avi" or ".mov" or ".mkv" or ".wmv";
        }

        public static string? ResolvePhysicalPath(string? relativeOrAbsolute)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolute)) return null;
            var p = relativeOrAbsolute.Trim();
            if (File.Exists(p)) return p;

            if (p.StartsWith("/") || p.StartsWith("\\"))
                p = p.TrimStart('/', '\\');

            var combined = Path.Combine(
                Application.StartupPath,
                p.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            return File.Exists(combined) ? combined : null;
        }

        public static Image? LoadImage(string? relativeOrAbsolute, int? placeholderW = null, int? placeholderH = null)
        {
            if (IsVideo(relativeOrAbsolute))
            {
                if (placeholderW.HasValue && placeholderH.HasValue)
                    return CreatePlaceholder(placeholderW.Value, placeholderH.Value, "▶ Video");
                return null;
            }
            var physical = ResolvePhysicalPath(relativeOrAbsolute);
            if (physical != null)
            {
                try
                {
                    // Copy vào memory để không khóa file
                    using var fs = new FileStream(physical, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return Image.FromStream(fs);
                }
                catch { /* fall through */ }
            }

            if (placeholderW.HasValue && placeholderH.HasValue)
                return CreatePlaceholder(placeholderW.Value, placeholderH.Value, "Không có ảnh");
            return null;
        }

        public static void ApplyToPictureBox(PictureBox pic, string? path, string emptyText = "Không có ảnh")
        {
            pic.Image?.Dispose();
            pic.Image = null;
            int w = pic.Width > 10 ? pic.Width : 280;
            int h = pic.Height > 10 ? pic.Height : 180;
            if (IsVideo(path))
            {
                pic.Image = CreatePlaceholder(w, h, "▶ Video — nhấn để mở");
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                return;
            }
            var img = LoadImage(path, w, h);
            if (img == null)
                img = CreatePlaceholder(w, h, emptyText);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Image = img;
        }

        /// <summary>Mở video bằng ứng dụng mặc định của Windows.</summary>
        public static void OpenMedia(string? path)
        {
            var physical = ResolvePhysicalPath(path);
            if (physical == null) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(physical) { UseShellExecute = true });
            }
            catch { /* ignore */ }
        }

        public static Image CreatePlaceholder(int width, int height, string text)
        {
            width = Math.Max(40, width);
            height = Math.Max(40, height);
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(241, 245, 249));
            using (var brush = new SolidBrush(Color.FromArgb(226, 232, 240)))
                g.FillRectangle(brush, 0, 0, width, height);
            using var pen = new Pen(Color.FromArgb(203, 213, 225), 1);
            g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
            // icon khung ảnh đơn giản
            int iw = Math.Min(64, width / 3);
            int ih = Math.Min(48, height / 3);
            var ir = new Rectangle((width - iw) / 2, (height - ih) / 2 - 10, iw, ih);
            using (var p2 = new Pen(Color.FromArgb(148, 163, 184), 2))
                g.DrawRectangle(p2, ir);
            using var font = new Font("Segoe UI", 9F);
            using var tb = new SolidBrush(Color.FromArgb(100, 116, 139));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            g.DrawString(text, font, tb, new RectangleF(8, ir.Bottom + 8, width - 16, 40), sf);
            return bmp;
        }

        /// <summary>Tạo ảnh demo đẹp cho sample data nếu file chưa tồn tại.</summary>
        public static void EnsureSampleImages(string appRoot)
        {
            var samples = new (string Rel, Color Accent, string Label)[]
            {
                ("uploads/rooms/101_1.jpg", Color.FromArgb(37, 99, 235), "Phòng 101"),
                ("uploads/rooms/101_2.jpg", Color.FromArgb(14, 165, 233), "Phòng 101 · 2"),
                ("uploads/rooms/102_1.jpg", Color.FromArgb(22, 163, 74), "Phòng 102"),
                ("uploads/posts/101_1.jpg", Color.FromArgb(37, 99, 235), "Tin đăng 101"),
                ("uploads/posts/101_2.jpg", Color.FromArgb(79, 70, 229), "Tin đăng 101 · 2"),
            };

            foreach (var (rel, accent, label) in samples)
            {
                var full = Path.Combine(appRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                var dir = Path.GetDirectoryName(full);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                if (File.Exists(full)) continue;
                using var bmp = CreateDemoRoomImage(640, 420, accent, label);
                bmp.Save(full, ImageFormat.Jpeg);
            }
        }

        private static Bitmap CreateDemoRoomImage(int w, int h, Color accent, string label)
        {
            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var lg = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(248, 250, 252),
                Color.FromArgb(accent.R, accent.G, accent.B),
                45f))
                g.FillRectangle(lg, 0, 0, w, h);

            using (var overlay = new SolidBrush(Color.FromArgb(90, 15, 23, 42)))
                g.FillRectangle(overlay, 0, h - 90, w, 90);

            using var font = new Font("Segoe UI", 22F, FontStyle.Bold);
            using var font2 = new Font("Segoe UI", 11F);
            using var white = new SolidBrush(Color.White);
            using var muted = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
            g.DrawString(label, font, white, 24, h - 78);
            g.DrawString("RPMS · Ảnh minh họa", font2, muted, 26, h - 40);
            return bmp;
        }
    }
}
