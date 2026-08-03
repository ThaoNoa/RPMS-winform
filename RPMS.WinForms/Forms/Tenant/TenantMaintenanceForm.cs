using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Maintenance;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantMaintenanceForm : Form
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IContractService _contractService;
        private ComboBox cboContract;
        private ModernTextBox txtTitle;
        private TextBox txtDesc;
        private PictureBox picPreview;
        private string _imagePath = "";

        public TenantMaintenanceForm(IMaintenanceService maintenanceService, IContractService contractService)
        {
            _maintenanceService = maintenanceService;
            _contractService = contractService;
            InitializeUI();
            this.Load += async (s, e) => await LoadContractsAsync();
        }

        private void InitializeUI()
        {
            this.ClientSize = new Size(800, 600);
            this.BackColor = AppColors.Card;
            this.Text = "Yêu cầu sửa chữa";

            Label lblH1 = new Label { Text = "Báo cáo sự cố / Yêu cầu bảo trì", Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(30, 20), AutoSize = true };
            Label lblContract = new Label { Text = "Phòng gặp sự cố:", Location = new Point(30, 80), AutoSize = true };
            cboContract = new ComboBox { Location = new Point(30, 110), Size = new Size(300, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblTitle = new Label { Text = "Loại sự cố (Tiêu đề):", Location = new Point(30, 160), AutoSize = true };
            txtTitle = new ModernTextBox { Location = new Point(30, 190), Size = new Size(400, 35) };
            Label lblDesc = new Label { Text = "Mô tả chi tiết:", Location = new Point(30, 240), AutoSize = true };
            txtDesc = new TextBox { Location = new Point(30, 270), Size = new Size(400, 120), Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            Label lblImg = new Label { Text = "Hình ảnh thực tế:", Location = new Point(480, 80), AutoSize = true };
            picPreview = new PictureBox { Location = new Point(480, 110), Size = new Size(250, 200), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            ModernButton btnUpload = new ModernButton { Text = "Chọn ảnh", Location = new Point(480, 320), Size = new Size(100, 35) };
            btnUpload.Click += BtnUpload_Click;
            ModernButton btnSubmit = new ModernButton { Text = "Gửi Yêu Cầu", Location = new Point(300, 450), Size = new Size(200, 45), BackColor = AppColors.Primary };
            btnSubmit.Click += BtnSubmit_Click;

            this.Controls.AddRange(new Control[] { lblH1, lblContract, cboContract, lblTitle, txtTitle, lblDesc, txtDesc, lblImg, picPreview, btnUpload, btnSubmit });
        }

        private async System.Threading.Tasks.Task LoadContractsAsync()
        {
            var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
            cboContract.DataSource = contracts.Where(c => c.Status == "Active").ToList();
            cboContract.DisplayMember = "RoomNumber";
            cboContract.ValueMember = "ContractID";
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _imagePath = ofd.FileName;
                    picPreview.ImageLocation = _imagePath;
                }
            }
        }

        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (cboContract.SelectedValue == null || string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                AppDialog.ShowInfo("Vui lòng chọn phòng và nhập tiêu đề sự cố.");
                return;
            }

            try
            {
                string finalPath = "";
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    string uploadFolder = Path.Combine(Application.StartupPath, "uploads", "maintenance");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string fileName = $"maint_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(_imagePath)}";
                    string destPath = Path.Combine(uploadFolder, fileName);
                    File.Copy(_imagePath, destPath, true);
                    finalPath = $"/uploads/maintenance/{fileName}";
                }

                await _maintenanceService.CreateRequestAsync(new CreateMaintenanceDto
                {
                    ContractID = (int)cboContract.SelectedValue,
                    Title = txtTitle.Text,
                    Description = txtDesc.Text,
                    ImagePath = finalPath
                });

                AppDialog.ShowInfo("Gửi yêu cầu bảo trì thành công! Quản lý sẽ sớm liên hệ với bạn.");
                txtTitle.Text = "";
                txtDesc.Text = "";
                _imagePath = "";
                picPreview.Image = null;
                picPreview.ImageLocation = null;
                await LoadContractsAsync();
            }
            catch (Exception ex) { AppDialog.ShowError("Lỗi: " + ex.Message); }
        }
    }
}