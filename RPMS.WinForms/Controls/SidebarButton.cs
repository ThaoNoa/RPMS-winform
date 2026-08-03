using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class SidebarButton : Button
    {
        private bool _isActive;
        private static readonly Font MenuFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        public SidebarButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = AppColors.Sidebar;
            ForeColor = Color.FromArgb(226, 232, 240);
            // Clone — không gán lại Font khi active/inactive
            Font = (Font)MenuFont.Clone();
            TextAlign = ContentAlignment.MiddleLeft;
            Padding = new Padding(20, 0, 0, 0);
            Size = new Size(250, 44);
            Cursor = Cursors.Hand;
            Margin = new Padding(0);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                SuspendLayout();
                try
                {
                    if (_isActive)
                    {
                        BackColor = AppColors.Primary;
                        ForeColor = Color.White;
                    }
                    else
                    {
                        BackColor = AppColors.Sidebar;
                        ForeColor = Color.FromArgb(226, 232, 240);
                    }
                }
                finally
                {
                    ResumeLayout(false);
                    Invalidate();
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_isActive)
            {
                BackColor = AppColors.SidebarHover;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isActive)
            {
                BackColor = AppColors.Sidebar;
                Invalidate();
            }
        }
    }
}
