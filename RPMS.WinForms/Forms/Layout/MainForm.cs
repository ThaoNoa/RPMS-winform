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
        private IServiceScope? _activeScope;
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
                    AddMenuButton("Đánh giá", "AdminReviews");
                    AddMenuButton("Nhật ký hệ thống", "ActivityLog");
                    AddMenuButton("Backup DB", "Backup");
                    break;
                case 2: // Landlord
                    AddMenuButton("Nhà của tôi", "LandlordHouse");
                    AddMenuButton("Phòng của tôi", "LandlordRoom");
                    AddMenuButton("Phân công Manager", "LandlordAssignment");
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
            IServiceScope? scope = null;
            try
            {
                // Mỗi màn hình một DI scope riêng — tránh DbContext dùng chung từ root SP
                // (concurrent async khi chuyển Dashboard → Tìm phòng / Yêu thích).
                scope = Program.ServiceProvider.CreateScope();
                var sp = scope.ServiceProvider;
                childForm = tag switch
                {
                    "Dashboard" => sp.GetRequiredService<Forms.Dashboard.DashboardForm>(),
                    "Notifications" => sp.GetRequiredService<Forms.Shared.NotificationCenterForm>(),
                    "Profile" => sp.GetRequiredService<Forms.Shared.ProfileForm>(),
                    "Chat" => sp.GetRequiredService<Forms.Shared.ChatForm>(),
                    "Calendar" => sp.GetRequiredService<Forms.Shared.CalendarForm>(),
                    "Reports" => sp.GetRequiredService<Forms.Shared.ReportForm>(),
                    "Backup" => sp.GetRequiredService<Forms.Admin.BackupForm>(),
                    "UserManagement" => sp.GetRequiredService<Forms.Admin.UserManagementForm>(),
                    "PostManagement" => sp.GetRequiredService<Forms.Admin.PostManagementForm>(),
                    "ActivityLog" => sp.GetRequiredService<Forms.Admin.ActivityLogForm>(),
                    "AdminReviews" => sp.GetRequiredService<Forms.Admin.ReviewManagementForm>(),
                    "LandlordHouse" => sp.GetRequiredService<Forms.Landlord.LandlordHouseForm>(),
                    "LandlordRoom" => sp.GetRequiredService<Forms.Landlord.LandlordRoomForm>(),
                    "LandlordAssignment" => sp.GetRequiredService<Forms.Landlord.LandlordAssignmentForm>(),
                    "LandlordContract" => sp.GetRequiredService<Forms.Landlord.LandlordContractForm>(),
                    "LandlordReviews" => sp.GetRequiredService<Forms.Landlord.LandlordReviewForm>(),
                    "TenantHome" => sp.GetRequiredService<Forms.Tenant.TenantHomeForm>(),
                    "TenantFavorite" => sp.GetRequiredService<Forms.Tenant.TenantFavoriteForm>(),
                    "TenantContract" => sp.GetRequiredService<Forms.Tenant.TenantContractForm>(),
                    "TenantReviews" => sp.GetRequiredService<Forms.Tenant.TenantReviewForm>(),
                    "ManagerMeter" => sp.GetRequiredService<Forms.Manager.ManagerMeterForm>(),
                    "LandlordAppointment" => sp.GetRequiredService<Forms.Landlord.LandlordAppointmentForm>(),
                    "LandlordPost" => sp.GetRequiredService<Forms.Landlord.LandlordPostForm>(),
                    "TenantInvoice" => sp.GetRequiredService<Forms.Tenant.TenantInvoiceForm>(),
                    "TenantMaintenance" => sp.GetRequiredService<Forms.Tenant.TenantMaintenanceForm>(),
                    "ManagerMaintenance" => sp.GetRequiredService<Forms.Manager.ManagerMaintenanceForm>(),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                try { scope?.Dispose(); } catch { /* ignore */ }
                var detail = ex.GetBaseException().Message;
                AppDialog.ShowError("Không mở được màn hình: " + detail);
                return;
            }

            if (childForm != null)
            {
                _activeScope = scope;
                OpenChildForm(childForm);
            }
            else
            {
                try { scope?.Dispose(); } catch { /* ignore */ }
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
            if (_activeForm == null && _activeScope == null) return;
            try
            {
                if (_activeForm != null)
                {
                    pnlContent.Controls.Remove(_activeForm);
                    _activeForm.Close();
                    _activeForm.Dispose();
                }
            }
            catch
            {
                /* child may already be disposed */
            }
            finally
            {
                _activeForm = null;
                pnlContent.Tag = null;
                try { _activeScope?.Dispose(); } catch { /* in-flight async may observe disposed DbContext */ }
                _activeScope = null;
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
