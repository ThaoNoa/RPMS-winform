using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DTO.User;
using RPMS.WinForms.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    public partial class UserModalForm : Form
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public bool IsEditMode { get; set; }
        public int UserIdToEdit { get; set; }

        public UserModalForm(IUserService userService, IRoleService roleService)
        {
            InitializeComponent();
            _userService = userService;
            _roleService = roleService;
            this.Load += UserModalForm_Load!;
        }

        private async void UserModalForm_Load(object sender, EventArgs e)
        {
            await LoadRolesAsync();
            if (IsEditMode)
            {
                lblTitle.Text = "Cập nhật Người dùng";
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
                txtPassword.Text = "**********";
                await LoadUserDetailsAsync();
            }
            else
            {
                lblTitle.Text = "Thêm Người dùng mới";
                cboStatus.SelectedIndex = 0;
                cboStatus.Enabled = false;
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                cboRole.DataSource = roles.ToList();
                cboRole.DisplayMember = "RoleName";
                cboRole.ValueMember = "RoleID";
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách quyền: " + ex.Message);
            }
        }

        private async Task LoadUserDetailsAsync()
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(UserIdToEdit);
                txtUsername.Text = user.Username;
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.Phone;
                txtAddress.Text = user.Address;
                cboRole.SelectedValue = user.RoleID;
                cboStatus.SelectedItem = user.Status;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải thông tin: " + ex.Message);
                this.Close();
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                cboRole.SelectedValue == null)
            {
                AppDialog.ShowWarning("Vui lòng điền đầy đủ các trường bắt buộc (*).");
                return;
            }

            btnSave.Enabled = false;
            try
            {
                if (IsEditMode)
                {
                    var updateRequest = new UpdateUserDto
                    {
                        RoleID = (int)cboRole.SelectedValue,
                        FullName = txtFullName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Address = txtAddress.Text.Trim(),
                        Status = cboStatus.SelectedItem.ToString()
                    };
                    await _userService.UpdateUserAsync(UserIdToEdit, updateRequest);
                    AppDialog.ShowInfo("Cập nhật thành công!");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        AppDialog.ShowWarning("Tên đăng nhập và mật khẩu không được để trống.");
                        btnSave.Enabled = true;
                        return;
                    }
                    var createRequest = new CreateUserDto
                    {
                        RoleID = (int)cboRole.SelectedValue,
                        Username = txtUsername.Text.Trim(),
                        Password = txtPassword.Text,
                        FullName = txtFullName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Address = txtAddress.Text.Trim()
                    };
                    await _userService.CreateUserAsync(createRequest);
                    AppDialog.ShowInfo("Thêm mới thành công!");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (BadRequestException ex)
            {
                AppDialog.ShowWarning(ex.Message, "Lỗi dữ liệu");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}