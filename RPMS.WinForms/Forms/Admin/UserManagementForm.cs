using RPMS.BLL.Interfaces;
using RPMS.DTO.User;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace RPMS.WinForms.Forms.Admin
{
    public partial class UserManagementForm : Form
    {
        private readonly IUserService _userService;
        private List<UserDto> _allUsers;

        public UserManagementForm(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            _allUsers = new List<UserDto>();
            SetupDataGridView();
            this.Load += UserManagementForm_Load!;
        }

        private void SetupDataGridView()
        {
            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserID", HeaderText = "ID", Width = 60 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", Width = 120 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoleName", HeaderText = "Vai trò", Width = 100 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 80 });
            dgvUsers.Columns.Add(new DataGridViewLinkColumn { HeaderText = "Thao tác", Text = "Sửa", UseColumnTextForLinkValue = true, Name = "EditCol", Width = 60 });
            dgvUsers.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Khóa/Mở", UseColumnTextForLinkValue = true, Name = "ToggleCol", Width = 70 });
            dgvUsers.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, Name = "DeleteCol", Width = 60 });
        }

        private async void UserManagementForm_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _allUsers = users.ToList();
                BindData(_allUsers);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void BindData(List<UserDto> data)
        {
            dgvUsers.DataSource = null;
            dgvUsers.DataSource = data;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(keyword))
                BindData(_allUsers);
            else
            {
                var filtered = _allUsers.Where(u =>
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Username.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword)).ToList();
                BindData(filtered);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var modal = Program.ServiceProvider.GetRequiredService<UserModalForm>();
            modal.IsEditMode = false;
            if (modal.ShowDialog() == DialogResult.OK)
                await LoadDataAsync();
        }

        private async void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var user = dgvUsers.Rows[e.RowIndex].DataBoundItem as UserDto;
            if (user == null) return;

            string colName = dgvUsers.Columns[e.ColumnIndex].Name;
            try
            {
                if (colName == "EditCol")
                {
                    var modal = Program.ServiceProvider.GetRequiredService<UserModalForm>();
                    modal.IsEditMode = true;
                    modal.UserIdToEdit = user.UserID;
                    if (modal.ShowDialog() == DialogResult.OK)
                        await LoadDataAsync();
                }
                else if (colName == "ToggleCol")
                {
                    if (AppDialog.Confirm($"Bạn có chắc chắn muốn thay đổi trạng thái của {user.FullName}?"))
                    {
                        await _userService.ToggleUserStatusAsync(user.UserID);
                        await LoadDataAsync();
                    }
                }
                else if (colName == "DeleteCol")
                {
                    if (AppDialog.Confirm($"Bạn có chắc chắn muốn XÓA {user.FullName}? (Hệ thống sẽ khóa tài khoản)"))
                    {
                        await _userService.DeleteUserAsync(user.UserID);
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Thao tác thất bại: " + ex.Message);
            }
        }
    }
}