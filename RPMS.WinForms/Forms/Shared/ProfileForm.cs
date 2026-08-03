using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Auth;
using RPMS.DTO.User;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class ProfileForm : Form
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IActivityLogService _activityLogService;

        private ModernTextBox txtFullName = null!;
        private ModernTextBox txtEmail = null!;
        private ModernTextBox txtPhone = null!;
        private ModernTextBox txtAddress = null!;
        private ModernTextBox txtOldPassword = null!;
        private ModernTextBox txtNewPassword = null!;
        private ModernTextBox txtConfirmPassword = null!;
        private ModernDataGridView dgvLogs = null!;

        public ProfileForm(IUserService userService, IAuthService authService, IActivityLogService activityLogService)
        {
            _userService = userService;
            _authService = authService;
            _activityLogService = activityLogService;
            InitializeUI();
            Load += async (s, e) => await LoadProfileAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Hồ sơ cá nhân";
            ClientSize = new Size(1000, 650);
            AutoScroll = true;

            var lblProfile = new Label
            {
                Text = "Thông tin cá nhân",
                Font = AppTypography.Heading,
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            };

            txtFullName = Field(30, 60, "Họ tên");
            txtEmail = Field(30, 120, "Email");
            txtPhone = Field(30, 180, "Số điện thoại");
            txtAddress = Field(30, 240, "Địa chỉ");

            var btnSave = new ModernButton
            {
                Text = "Lưu thông tin",
                Location = new Point(30, 310),
                Size = new Size(160, 38)
            };
            btnSave.Click += async (s, e) => await SaveProfileAsync();

            var lblPwd = new Label
            {
                Text = "Đổi mật khẩu",
                Font = AppTypography.Heading,
                Location = new Point(420, 20),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            };
            txtOldPassword = Field(420, 60, "Mật khẩu cũ");
            txtOldPassword.UseSystemPasswordChar = true;
            txtNewPassword = Field(420, 120, "Mật khẩu mới");
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmPassword = Field(420, 180, "Xác nhận mật khẩu mới");
            txtConfirmPassword.UseSystemPasswordChar = true;

            var btnChangePwd = new ModernButton
            {
                Text = "Đổi mật khẩu",
                Location = new Point(420, 250),
                Size = new Size(160, 38),
                BackColor = AppColors.Warning
            };
            btnChangePwd.Click += async (s, e) => await ChangePasswordAsync();

            var lblLog = new Label
            {
                Text = "Hoạt động gần đây",
                Font = AppTypography.Heading,
                Location = new Point(30, 370),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            };

            dgvLogs = new ModernDataGridView
            {
                Location = new Point(30, 410),
                Size = new Size(920, 200)
            };
            dgvLogs.AutoGenerateColumns = false;
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "Hành động", Width = 140 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Details", HeaderText = "Chi tiết", Width = 520 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            Controls.Add(lblProfile);
            Controls.Add(btnSave);
            Controls.Add(lblPwd);
            Controls.Add(btnChangePwd);
            Controls.Add(lblLog);
            Controls.Add(dgvLogs);
        }

        private ModernTextBox Field(int x, int y, string label)
        {
            Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = AppColors.TextMuted
            });
            var txt = new ModernTextBox { Location = new Point(x, y + 22), Size = new Size(320, 34) };
            Controls.Add(txt);
            return txt;
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(UserSession.CurrentUser!.UserID);
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.Phone;
                txtAddress.Text = user.Address;

                var logs = await _activityLogService.GetByUserAsync(user.UserID);
                dgvLogs.DataSource = logs.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task SaveProfileAsync()
        {
            try
            {
                var current = await _userService.GetUserByIdAsync(UserSession.CurrentUser!.UserID);
                await _userService.UpdateUserAsync(current.UserID, new UpdateUserDto
                {
                    RoleID = current.RoleID,
                    FullName = txtFullName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    Status = current.Status
                });
                await _activityLogService.LogAsync(current.UserID, "UpdateProfile", "Cập nhật hồ sơ cá nhân");
                AppDialog.ShowInfo("Đã lưu thông tin hồ sơ.");
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                await _authService.ChangePasswordAsync(UserSession.CurrentUser!.UserID, new ChangePasswordDto
                {
                    OldPassword = txtOldPassword.Text,
                    NewPassword = txtNewPassword.Text,
                    ConfirmNewPassword = txtConfirmPassword.Text
                });
                await _activityLogService.LogAsync(UserSession.CurrentUser.UserID, "ChangePassword", "Đổi mật khẩu thành công");
                txtOldPassword.Text = "";
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";
                AppDialog.ShowInfo("Đổi mật khẩu thành công.");
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
