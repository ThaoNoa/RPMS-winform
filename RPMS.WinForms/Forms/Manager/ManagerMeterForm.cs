using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Contract;
using RPMS.DTO.Invoice;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Manager
{
    public class ManagerMeterForm : Form
    {
        private readonly IContractService _contractService;
        private readonly IInvoiceService _invoiceService;
        private ModernDataGridView dgvContracts = null!;
        private Panel pnlInput = null!;
        private Label lblSelectedRoom = null!;
        private ModernTextBox txtElectric = null!;
        private ModernTextBox txtWater = null!;
        private ModernTextBox txtFee = null!;
        private ModernButton btnGenerateInvoice = null!;
        private int _selectedContractId = 0;

        public ManagerMeterForm(IContractService contractService, IInvoiceService invoiceService)
        {
            _contractService = contractService;
            _invoiceService = invoiceService;
            InitializeUI();
            Load += ManagerMeterForm_Load!;
        }

        private void InitializeUI()
        {
            ClientSize = new Size(1000, 650);
            BackColor = AppColors.Background;
            Text = "Ghi chỉ số Điện Nước";

            pnlInput = new Panel { Dock = DockStyle.Right, Width = 350, BackColor = AppColors.Card, Padding = new Padding(20) };

            var lblTitle = new Label
            {
                Text = "Chốt số & Tạo Hóa đơn",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 20),
                AutoSize = true
            };
            lblSelectedRoom = new Label
            {
                Text = "Chưa chọn hợp đồng",
                Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                ForeColor = AppColors.TextMuted,
                Location = new Point(20, 60),
                AutoSize = true
            };
            var lblElectric = new Label { Text = "Chỉ số điện MỚI *", Location = new Point(20, 110), AutoSize = true };
            txtElectric = new ModernTextBox { Location = new Point(20, 135), Size = new Size(300, 35) };
            var lblWater = new Label { Text = "Chỉ số nước MỚI *", Location = new Point(20, 180), AutoSize = true };
            txtWater = new ModernTextBox { Location = new Point(20, 205), Size = new Size(300, 35) };
            var lblFee = new Label { Text = "Phụ phí khác (nếu có)", Location = new Point(20, 250), AutoSize = true };
            txtFee = new ModernTextBox { Location = new Point(20, 275), Size = new Size(300, 35), Text = "0" };

            btnGenerateInvoice = new ModernButton
            {
                Text = "Tạo Hóa Đơn",
                Location = new Point(20, 340),
                Size = new Size(300, 45),
                BackColor = AppColors.Primary,
                Enabled = false
            };
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click!;

            pnlInput.Controls.AddRange(new Control[] { lblTitle, lblSelectedRoom, lblElectric, txtElectric, lblWater, txtWater, lblFee, txtFee, btnGenerateInvoice });

            dgvContracts = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvContracts.AutoGenerateColumns = false;
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã hợp đồng", Width = 120 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 100 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", Width = 150 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MonthlyRent", HeaderText = "Tiền thuê", Width = 100 });
            dgvContracts.CellClick += DgvContracts_CellClick!;

            Controls.Add(dgvContracts);
            Controls.Add(pnlInput);
        }

        private async void ManagerMeterForm_Load(object sender, EventArgs e)
        {
            await LoadActiveContractsAsync();
        }

        private async Task LoadActiveContractsAsync()
        {
            try
            {
                var contracts = await _contractService.GetContractsByManagerAsync(UserSession.CurrentUser!.UserID);
                var activeContracts = contracts.Where(c => c.Status == "Active").ToList();
                dgvContracts.DataSource = activeContracts;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách hợp đồng: " + ex.Message);
            }
        }

        private void DgvContracts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var contract = dgvContracts.Rows[e.RowIndex].DataBoundItem as ContractDto;
                if (contract != null)
                {
                    _selectedContractId = contract.ContractID;
                    lblSelectedRoom.Text = $"Phòng {contract.RoomNumber} - {contract.TenantName}";
                    btnGenerateInvoice.Enabled = true;
                }
            }
        }

        private async void BtnGenerateInvoice_Click(object sender, EventArgs e)
        {
            if (_selectedContractId == 0) return;
            if (!decimal.TryParse(txtElectric.Text, out decimal newElectric) ||
                !decimal.TryParse(txtWater.Text, out decimal newWater) ||
                !decimal.TryParse(txtFee.Text, out decimal otherFee))
            {
                AppDialog.ShowWarning("Vui lòng nhập số hợp lệ cho điện, nước và phụ phí.");
                return;
            }

            btnGenerateInvoice.Enabled = false;
            try
            {
                var request = new GenerateInvoiceDto
                {
                    ContractID = _selectedContractId,
                    ReadingMonth = DateTime.Now,
                    NewElectric = newElectric,
                    NewWater = newWater,
                    OtherFee = otherFee,
                    CreatedBy = UserSession.CurrentUser!.UserID
                };
                await _invoiceService.GenerateMonthlyInvoiceAsync(request);
                AppDialog.ShowInfo("Tạo hóa đơn thành công! Khách thuê đã có thể xem và thanh toán.");
                txtElectric.Text = "";
                txtWater.Text = "";
                txtFee.Text = "0";
                lblSelectedRoom.Text = "Chưa chọn hợp đồng";
                _selectedContractId = 0;
                btnGenerateInvoice.Enabled = false;
                await LoadActiveContractsAsync();
            }
            catch (BadRequestException ex)
            {
                AppDialog.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                btnGenerateInvoice.Enabled = true;
            }
        }
    }
}
