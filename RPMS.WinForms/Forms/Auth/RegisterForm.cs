using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DTO.Role;
using RPMS.DTO.User;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Auth
{
    public partial class RegisterForm : Form
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private List<RoleDto> _roles = new();

        public string? RegisteredUsername { get; private set; }

        public RegisterForm(IUserService userService, IRoleService roleService)
        {
            InitializeComponent();
            _userService = userService;
            _roleService = roleService;
            AcceptButton = btnRegister;
            Load += RegisterForm_Load;
        }

        private async void RegisterForm_Load(object? sender, EventArgs e)
        {
            await LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var allRoles = await _roleService.GetAllRolesAsync();
                var allowedRoleNames = new[] { "Landlord", "Tenant", "Manager" };
                _roles = allRoles.Where(r => allowedRoleNames.Contains(r.RoleName, StringComparer.OrdinalIgnoreCase)).ToList();
                cboRole.DataSource = _roles;
                cboRole.DisplayMember = nameof(RoleDto.RoleName);
                cboRole.ValueMember = nameof(RoleDto.RoleID);
                if (cboRole.Items.Count > 0)
                    cboRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách quyền: " + ex.Message);
            }
        }

        private async void btnRegister_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm) ||
                string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || cboRole.SelectedValue == null)
            {
                ShowError("Vui lòng điền đầy đủ các trường bắt buộc (*).");
                return;
            }

            if (!email.Contains('@') || email.Length < 5)
            {
                ShowError("Email không hợp lệ.");
                return;
            }

            if (password != confirm)
            {
                ShowError("Mật khẩu xác nhận không khớp.");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự.");
                return;
            }

            btnRegister.Enabled = false;
            btnRegister.Text = "Đang tạo tài khoản...";
            lblErrorMessage.Visible = false;

            try
            {
                await _userService.CreateUserAsync(new CreateUserDto
                {
                    RoleID = Convert.ToInt32(cboRole.SelectedValue),
                    Username = username,
                    Password = password,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Address = address
                });

                RegisteredUsername = username;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (BadRequestException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Tạo tài khoản";
            }
        }

        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visible = true;
        }

        private void lblLoginLink_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
