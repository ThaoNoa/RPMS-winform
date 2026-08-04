using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DTO.Auth;
using RPMS.WinForms.UI;
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace RPMS.WinForms.Forms.Auth
{
    public partial class LoginForm : Form
    {
        private readonly IAuthService _authService;

        public LoginForm(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            AcceptButton = btnLogin;
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";
            lblErrorMessage.Visible = false;

            try
            {
                var response = await _authService.LoginAsync(new LoginRequestDto
                {
                    Username = username,
                    Password = password
                });
                UserSession.Login(response);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (UnauthorizedException ex)
            {
                ShowError(ex.Message);
            }
            catch
            {
                ShowError("Không kết nối được hệ thống. Kiểm tra database LocalDB rồi thử lại.");
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }

        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visible = true;
        }

        private void lblRegisterLink_Click(object? sender, EventArgs e)
        {
            var registerForm = Program.ServiceProvider.GetRequiredService<RegisterForm>();
            var result = registerForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                lblErrorMessage.Visible = false;
                if (!string.IsNullOrWhiteSpace(registerForm.RegisteredUsername))
                {
                    txtUsername.Text = registerForm.RegisteredUsername;
                    txtPassword.Text = "";
                    txtPassword.Focus();
                }
                AppDialog.ShowInfo("Đăng ký thành công. Hãy đăng nhập bằng tài khoản vừa tạo.");
            }
        }

        private void lblBrandDesc_Click(object sender, EventArgs e)
        {

        }
    }
}
