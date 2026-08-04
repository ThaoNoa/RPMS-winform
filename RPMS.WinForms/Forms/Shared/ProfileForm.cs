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
            Text = "Hồ sơ cá nhân";
            ClientSize = new Size(1000, 650);

            var header = UIHelper.CreatePageHeader("Hồ sơ cá nhân");

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 120,
                Panel2MinSize = 120,
                BackColor = AppColors.Background
            };
            void SafeSplit()
            {
                try
                {
                    int max = split.Height - split.Panel2MinSize - split.SplitterWidth;
                    if (max > split.Panel1MinSize)
                        split.SplitterDistance = Math.Min(320, max);
                }
                catch { }
            }
            Load += (_, _) => SafeSplit();
            split.SizeChanged += (_, _) => SafeSplit();

            var pnlFields = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(AppLayout.PagePadding),
                BackColor = AppColors.Background,
                AutoScroll = true
            };

            var tblTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                BackColor = AppColors.Background
            };
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tblTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var tblProfile = BuildFieldColumn("Thông tin cá nhân", out txtFullName, out txtEmail, out txtPhone, out txtAddress,
                ("Họ tên", null), ("Email", null), ("Số điện thoại", null), ("Địa chỉ", null));
            tblProfile.Padding = new Padding(0, 0, 16, 0);

            var tblPwd = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(16, 0, 0, 0)
            };
            tblPwd.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int pwdRow = 0;
            void AddPwdRow(Control c)
            {
                tblPwd.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                c.Dock = DockStyle.Top;
                tblPwd.Controls.Add(c, 0, pwdRow++);
            }

            AddPwdRow(new Label
            {
                Text = "Đổi mật khẩu",
                Font = AppTypography.Heading,
                AutoSize = true,
                ForeColor = AppColors.TextMain,
                Margin = new Padding(0, 0, 0, 8)
            });

            txtOldPassword = AddFieldToColumn(tblPwd, ref pwdRow, "Mật khẩu cũ");
            txtOldPassword.UseSystemPasswordChar = true;
            txtNewPassword = AddFieldToColumn(tblPwd, ref pwdRow, "Mật khẩu mới");
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmPassword = AddFieldToColumn(tblPwd, ref pwdRow, "Xác nhận mật khẩu mới");
            txtConfirmPassword.UseSystemPasswordChar = true;

            var btnChangePwd = UIHelper.PrimaryButton("Đổi mật khẩu", 160);
            btnChangePwd.BackColor = AppColors.Warning;
            btnChangePwd.Margin = new Padding(0, 8, 0, 0);
            btnChangePwd.Click += async (s, e) => await ChangePasswordAsync();
            AddPwdRow(btnChangePwd);

            var btnSave = UIHelper.PrimaryButton("Lưu thông tin", 160);
            btnSave.Margin = new Padding(0, 8, 0, 0);
            btnSave.Click += async (s, e) => await SaveProfileAsync();
            tblProfile.Controls.Add(btnSave, 0, tblProfile.RowCount);
            tblProfile.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tblTop.Controls.Add(tblProfile, 0, 0);
            tblTop.Controls.Add(tblPwd, 1, 0);
            pnlFields.Controls.Add(tblTop);
            split.Panel1.Controls.Add(pnlFields);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(AppLayout.PagePadding, 8, AppLayout.PagePadding, AppLayout.PagePadding),
                BackColor = AppColors.Background
            };

            var lblLog = new Label
            {
                Text = "Hoạt động gần đây",
                Font = AppTypography.Heading,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = AppColors.TextMain,
                Padding = new Padding(0, 0, 0, 8)
            };

            dgvLogs = new ModernDataGridView();
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "Hành động", FillWeight = 18 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Details", HeaderText = "Chi tiết", FillWeight = 52 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            UIHelper.ApplyGridFill(dgvLogs);

            pnlGrid.Controls.Add(dgvLogs);
            pnlGrid.Controls.Add(lblLog);
            split.Panel2.Controls.Add(pnlGrid);

            Controls.Add(split);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, split);
        }

        private static TableLayoutPanel BuildFieldColumn(string heading, out ModernTextBox txt1, out ModernTextBox txt2, out ModernTextBox txt3, out ModernTextBox txt4, params (string label, object? _)[] fields)
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int row = 0;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = heading,
                Font = AppTypography.Heading,
                AutoSize = true,
                ForeColor = AppColors.TextMain,
                Margin = new Padding(0, 0, 0, 8)
            }, 0, row++);

            txt1 = AddFieldToColumn(tbl, ref row, fields[0].label);
            txt2 = AddFieldToColumn(tbl, ref row, fields[1].label);
            txt3 = AddFieldToColumn(tbl, ref row, fields[2].label);
            txt4 = AddFieldToColumn(tbl, ref row, fields[3].label);
            return tbl;
        }

        private static ModernTextBox AddFieldToColumn(TableLayoutPanel tbl, ref int row, string label)
        {
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = label,
                Font = AppTypography.Caption,
                AutoSize = true,
                ForeColor = AppColors.TextMuted,
                Margin = new Padding(0, 8, 0, 4)
            }, 0, row);

            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var txt = new ModernTextBox { Height = AppLayout.InputHeight, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 0) };
            tbl.Controls.Add(txt, 0, row + 1);
            row += 2;
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
