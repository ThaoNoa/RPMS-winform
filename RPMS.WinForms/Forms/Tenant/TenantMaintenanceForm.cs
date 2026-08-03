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
            UIHelper.ApplyResizableDialog(this, new Size(640, 480));
            this.ClientSize = new Size(800, 600);
            this.BackColor = AppColors.Card;
            this.Text = "Yêu cầu sửa chữa";
            this.AutoScroll = false;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 10, 16, 10)
            };
            var btnSubmit = new ModernButton { Text = "Gửi Yêu Cầu", Size = new Size(200, 45), BackColor = AppColors.Primary };
            btnSubmit.Click += BtnSubmit_Click;
            var flpBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            flpBottom.Controls.Add(btnSubmit);
            pnlBottom.Controls.Add(flpBottom);

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = AppColors.Card
            };

            var tblMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            tblMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            tblMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            tblMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var tblLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(0, 0, 12, 0)
            };
            tblLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int leftRow = 0;
            void AddLeftRow(Control c, float height = 0, bool auto = true)
            {
                tblLeft.RowStyles.Add(auto
                    ? new RowStyle(SizeType.AutoSize)
                    : new RowStyle(SizeType.Absolute, height));
                c.Dock = DockStyle.Fill;
                tblLeft.Controls.Add(c, 0, leftRow++);
            }

            AddLeftRow(new Label
            {
                Text = "Báo cáo sự cố / Yêu cầu bảo trì",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(440, 0),
                Margin = new Padding(0, 0, 0, 12)
            });

            AddLeftRow(new Label { Text = "Phòng gặp sự cố:", AutoSize = true, Margin = new Padding(0, 0, 0, 4) });
            cboContract = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 30 };
            AddLeftRow(cboContract, 30, false);

            AddLeftRow(new Label { Text = "Loại sự cố (Tiêu đề):", AutoSize = true, Margin = new Padding(0, 12, 0, 4) });
            txtTitle = new ModernTextBox { Height = 35 };
            AddLeftRow(txtTitle, 35, false);

            AddLeftRow(new Label { Text = "Mô tả chi tiết:", AutoSize = true, Margin = new Padding(0, 12, 0, 4) });
            txtDesc = new TextBox { Multiline = true, BorderStyle = BorderStyle.FixedSingle, ScrollBars = ScrollBars.Vertical };
            AddLeftRow(txtDesc, 120, false);

            var tblRight = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            tblRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tblRight.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tblRight.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tblRight.Controls.Add(new Label
            {
                Text = "Hình ảnh thực tế:",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            }, 0, 0);

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                MinimumSize = new Size(200, 160)
            };
            tblRight.Controls.Add(picPreview, 0, 1);

            var btnUpload = new ModernButton { Text = "Chọn ảnh", Size = new Size(100, 35), Margin = new Padding(0, 8, 0, 0) };
            btnUpload.Click += BtnUpload_Click;
            var flpUpload = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 0)
            };
            flpUpload.Controls.Add(btnUpload);
            tblRight.Controls.Add(flpUpload, 0, 2);

            tblMain.Controls.Add(tblLeft, 0, 0);
            tblMain.Controls.Add(tblRight, 1, 0);
            root.Controls.Add(tblMain);

            this.Controls.Add(root);
            this.Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadContractsAsync()
        {
            try
            {
                var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
                if (IsDisposed) return;
                cboContract.DisplayMember = "RoomNumber";
                cboContract.ValueMember = "ContractID";
                cboContract.DataSource = contracts.Where(c => c.Status == "Active").ToList();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải được hợp đồng: " + ex.Message);
            }
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
                    ContractID = Convert.ToInt32(cboContract.SelectedValue),
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
