using Microsoft.Extensions.DependencyInjection;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Threading.Tasks;
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

            try
            {
                if (_currentActiveButton != null)
                    _currentActiveButton.IsActive = false;
                clickedBtn.IsActive = true;
                _currentActiveButton = clickedBtn;

                lblPageTitle.Text = clickedBtn.Text;
                string tag = clickedBtn.Tag?.ToString() ?? "";
                LoadChildForm(tag);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không mở được menu: " + ex.Message);
            }
        }

        private void LoadChildForm(string tag)
        {
            CloseActiveChild();

            Form? childForm = null;
            try
            {
                childForm = tag switch
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
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không mở được màn hình: " + ex.Message);
                return;
            }

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

        private void CloseActiveChild()
        {
            if (_activeForm == null) return;
            try
            {
                pnlContent.Controls.Remove(_activeForm);
                _activeForm.Close();
                _activeForm.Dispose();
            }
            catch
            {
                /* child may already be disposed */
            }
            finally
            {
                _activeForm = null;
                pnlContent.Tag = null;
            }
        }

        private void OpenChildForm(Form childForm)
        {
            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            // Layout do form con tự Dock/Anchor — tránh AutoScroll form gây khoảng trắng / cắt
            childForm.AutoScroll = false;
            childForm.MinimumSize = Size.Empty;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(childForm);
            pnlContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            if (!AppDialog.Confirm("Bạn có chắc chắn muốn đăng xuất?"))
                return;

            btnLogout.Enabled = false;
            Cursor = Cursors.WaitCursor;

            // Đóng form con trước (dừng Chat timer, giải phóng UI) — không chờ DB
            CloseActiveChild();

            int? userId = UserSession.CurrentUser?.UserID;
            UserSession.Logout();

            // Ghi log nền, tối đa ~800ms rồi vẫn thoát — không block UI bằng GetResult()
            if (userId.HasValue)
            {
                try
                {
                    using var scope = Program.ServiceProvider.CreateScope();
                    var logService = scope.ServiceProvider.GetRequiredService<RPMS.BLL.Interfaces.IActivityLogService>();
                    var logTask = logService.LogAsync(userId.Value, "Logout", "Đăng xuất khỏi hệ thống");
                    await Task.WhenAny(logTask, Task.Delay(800));
                }
                catch
                {
                    /* ignore logging errors on logout */
                }
            }

            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
