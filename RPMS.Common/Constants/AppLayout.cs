using System.Drawing;

namespace RPMS.Common.Constants
{
    /// <summary>Kích thước / khoảng cách chuẩn cho WinForms RPMS.</summary>
    public static class AppLayout
    {
        public const int PageHeaderHeight = 56;
        public const int ToolbarHeight = 72;
        public const int PagePadding = 16;
        public const int FieldGap = 12;
        public const int ButtonHeight = 40;
        public const int ButtonMinWidth = 110;
        public const int SidePanelWidth = 360;
        public const int InputHeight = 36;
        public const int ComboHeight = 32;
        public static readonly Size DialogMin = new(640, 520);
        public static readonly Size PageMin = new(780, 480);
    }
}
