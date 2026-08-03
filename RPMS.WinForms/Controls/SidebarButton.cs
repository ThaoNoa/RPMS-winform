using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class SidebarButton : Button
    {
        private bool _isActive;

        public SidebarButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = AppColors.Sidebar;
            ForeColor = Color.FromArgb(226, 232, 240);
            Font = AppTypography.Body;
            TextAlign = ContentAlignment.MiddleLeft;
            Padding = new Padding(20, 0, 0, 0);
            Size = new Size(250, 44);
            Cursor = Cursors.Hand;
            Margin = new Padding(0);
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    BackColor = AppColors.Primary;
                    ForeColor = Color.White;
                    Font = AppTypography.BodyBold;
                }
                else
                {
                    BackColor = AppColors.Sidebar;
                    ForeColor = Color.FromArgb(226, 232, 240);
                    Font = AppTypography.Body;
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_isActive)
                BackColor = AppColors.SidebarHover;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isActive)
                BackColor = AppColors.Sidebar;
        }
    }
}
