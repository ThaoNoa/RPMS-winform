using Microsoft.Extensions.DependencyInjection;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Layout
{
    public partial class MainForm : Form
    {
        private Form? _activeForm;
        private SidebarButton? _currentActiveButton;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            GenerateMenu();
        }

        private void SetupUI()
        {
            if (UserSession.CurrentUser != null)
            {
                lblUserInfo.Text = $"Xin chào, {UserSession.CurrentUser.FullName} ({UserSession.CurrentUser.RoleName})";
            }
        }

        private void GenerateMenu()
        {
            flpMenu.Controls.Clear();
            if (UserSession.CurrentUser == null) return;

            int roleId = UserSession.CurrentUser.RoleID;
            AddMenuButton("Dashboard", "Dashboard");
            AddMenuButton("Thông báo", "Notifications");
            AddMenuButton("Hồ sơ", "Profile");
            AddMenuButton("Lịch", "Calendar");
            if (roleId == 1 || roleId == 2)
                AddMenuButton("Báo cáo", "Reports");
            if (roleId == 2 || roleId == 3)
                AddMenuButton("Chat", "Chat");

            switch (roleId)
            {
                case 1: // Admin
                    AddMenuButton("Quản lý người dùng", "UserManagement");
                    AddMenuButton("Quản lý tin đăng", "PostManagement");
                    AddMenuButton("Phân công Manager", "Assignment");
                    AddMenuButton("Đánh giá", "AdminReviews");
                    AddMenuButton("Nhật ký hệ thống", "ActivityLog");
                    AddMenuButton("Backup DB", "Backup");
                    break;
                case 2: // Landlord
                    AddMenuButton("Nhà của tôi", "LandlordHouse");
                    AddMenuButton("Phòng của tôi", "LandlordRoom");
                    AddMenuButton("Hợp đồng", "LandlordContract");
                    AddMenuButton("Lịch hẹn xem phòng", "LandlordAppointment");
                    AddMenuButton("Đăng tin mới", "LandlordPost");
                    AddMenuButton("Đánh giá", "LandlordReviews");
                    break;
                case 3: // Tenant
                    AddMenuButton("Tìm phòng", "TenantHome");
                    AddMenuButton("Yêu thích", "TenantFavorite");
                    AddMenuButton("Hợp đồng của tôi", "TenantContract");
                    AddMenuButton("Hóa đơn của tôi", "TenantInvoice");
                    AddMenuButton("Báo sự cố", "TenantMaintenance");
                    AddMenuButton("Đánh giá", "TenantReviews");
                    break;
                case 4: // Manager
                    AddMenuButton("Ghi chỉ số điện nước", "ManagerMeter");
                    AddMenuButton("Quản lý sự cố", "ManagerMaintenance");
                    break;
            }

            if (flpMenu.Controls.Count > 0 && flpMenu.Controls[0] is SidebarButton firstBtn)
                firstBtn.PerformClick();
        }

        private void AddMenuButton(string text, string tag)
        {
            var btn = new SidebarButton { Text = text, Tag = tag };
            btn.Click += MenuButton_Click!;
            flpMenu.Controls.Add(btn);
        }

        private void MenuButton_Click(object? sender, EventArgs e)
        {
            var clickedBtn = sender as SidebarButton;
            if (clickedBtn == null || clickedBtn == _currentActiveButton) return;

            if (_currentActiveButton != null)
                _currentActiveButton.IsActive = false;
            clickedBtn.IsActive = true;
            _currentActiveButton = clickedBtn;

            lblPageTitle.Text = clickedBtn.Text;
            string tag = clickedBtn.Tag?.ToString() ?? "";
            LoadChildForm(tag);
        }

        private void LoadChildForm(string tag)
        {
            _activeForm?.Close();
            Form? childForm = tag switch
            {
                "Dashboard" => Program.ServiceProvider.GetRequiredService<Forms.Dashboard.DashboardForm>(),
                "Notifications" => Program.ServiceProvider.GetRequiredService<Forms.Shared.NotificationCenterForm>(),
                "Profile" => Program.ServiceProvider.GetRequiredService<Forms.Shared.ProfileForm>(),
                "Chat" => Program.ServiceProvider.GetRequiredService<Forms.Shared.ChatForm>(),
                "Calendar" => Program.ServiceProvider.GetRequiredService<Forms.Shared.CalendarForm>(),
                "Reports" => Program.ServiceProvider.GetRequiredService<Forms.Shared.ReportForm>(),
                "Backup" => Program.ServiceProvider.GetRequiredService<Forms.Admin.BackupForm>(),
                "UserManagement" => Program.ServiceProvider.GetRequiredService<Forms.Admin.UserManagementForm>(),
                "PostManagement" => Program.ServiceProvider.GetRequiredService<Forms.Admin.PostManagementForm>(),
                "Assignment" => Program.ServiceProvider.GetRequiredService<Forms.Admin.AssignmentManagementForm>(),
                "ActivityLog" => Program.ServiceProvider.GetRequiredService<Forms.Admin.ActivityLogForm>(),
                "AdminReviews" => Program.ServiceProvider.GetRequiredService<Forms.Admin.ReviewManagementForm>(),
                "LandlordHouse" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordHouseForm>(),
                "LandlordRoom" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordRoomForm>(),
                "LandlordContract" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordContractForm>(),
                "LandlordReviews" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordReviewForm>(),
                "TenantHome" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantHomeForm>(),
                "TenantFavorite" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantFavoriteForm>(),
                "TenantContract" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantContractForm>(),
                "TenantReviews" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantReviewForm>(),
                "ManagerMeter" => Program.ServiceProvider.GetRequiredService<Forms.Manager.ManagerMeterForm>(),
                "LandlordAppointment" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordAppointmentForm>(),
                "LandlordPost" => Program.ServiceProvider.GetRequiredService<Forms.Landlord.LandlordPostForm>(),
                "TenantInvoice" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantInvoiceForm>(),
                "TenantMaintenance" => Program.ServiceProvider.GetRequiredService<Forms.Tenant.TenantMaintenanceForm>(),
                "ManagerMaintenance" => Program.ServiceProvider.GetRequiredService<Forms.Manager.ManagerMaintenanceForm>(),
                _ => null
            };

            if (childForm != null)
            {
                OpenChildForm(childForm);
            }
            else
            {
                pnlContent.Controls.Clear();
                var lblPlaceholder = new Label
                {
                    Text = "Đang xây dựng: " + tag,
                    Font = AppTypography.Heading,
                    ForeColor = AppColors.TextMuted,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlContent.Controls.Add(lblPlaceholder);
            }
        }

        private void OpenChildForm(Form childForm)
        {
            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(childForm);
            pnlContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (!AppDialog.Confirm("Bạn có chắc chắn muốn đăng xuất?"))
                return;

            try
            {
                if (UserSession.CurrentUser != null)
                {
                    var logService = Program.ServiceProvider.GetRequiredService<RPMS.BLL.Interfaces.IActivityLogService>();
                    logService.LogAsync(UserSession.CurrentUser.UserID, "Logout", "Đăng xuất khỏi hệ thống")
                        .GetAwaiter().GetResult();
                }
            }
            catch { /* ignore logging errors on logout */ }

            UserSession.Logout();
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
