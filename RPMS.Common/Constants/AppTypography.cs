using System.Drawing;

namespace RPMS.Common.Constants
{
    /// <summary>
    /// Typography an toàn cho WinForms.
    /// Core fonts không bao giờ Dispose; mỗi lần lấy là Clone() để Control tự quản lý.
    /// </summary>
    public static class AppTypography
    {
        private static readonly Font TitleCore = Create("Segoe UI", 22F, FontStyle.Bold);
        private static readonly Font SubtitleCore = Create("Segoe UI", 18F, FontStyle.Bold);
        private static readonly Font HeadingCore = Create("Segoe UI", 14F, FontStyle.Bold);
        private static readonly Font BodyCore = Create("Segoe UI", 10F, FontStyle.Regular);
        private static readonly Font BodyBoldCore = Create("Segoe UI", 10F, FontStyle.Bold);
        private static readonly Font CaptionCore = Create("Segoe UI", 9F, FontStyle.Regular);
        private static readonly Font ButtonCore = Create("Segoe UI", 10F, FontStyle.Bold);

        public static Font Title => (Font)TitleCore.Clone();
        public static Font Subtitle => (Font)SubtitleCore.Clone();
        public static Font Heading => (Font)HeadingCore.Clone();
        public static Font Body => (Font)BodyCore.Clone();
        public static Font BodyBold => (Font)BodyBoldCore.Clone();
        public static Font Caption => (Font)CaptionCore.Clone();
        public static Font Button => (Font)ButtonCore.Clone();

        private static Font Create(string family, float size, FontStyle style)
        {
            try
            {
                return new Font(family, size, style, GraphicsUnit.Point);
            }
            catch
            {
                return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
            }
        }
    }
}
