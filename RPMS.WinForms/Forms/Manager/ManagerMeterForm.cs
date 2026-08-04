using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgvContracts = null!;
        private Panel pnlList = null!;
        private Panel pnlInput = null!;
        private EmptyStatePanel emptyState = null!;
        private Label lblSelectedRoom = null!;
        private Label lblBillingMonth = null!;
        private Label lblPrevMonth = null!;
        private Label lblOldElectric = null!;
        private Label lblOldWater = null!;
        private ModernTextBox txtElectric = null!;
        private ModernTextBox txtWater = null!;
        private ModernTextBox txtFee = null!;
        private ModernButton btnGenerateInvoice = null!;
        private int _selectedContractId = 0;
        private decimal _prevElectric = 0;
        private decimal _prevWater = 0;

        /// <summary>Tháng hóa đơn = tháng trước (tháng đã kết thúc).</summary>
        private static DateTime BillingMonthStart
        {
            get
            {
                var prev = DateTime.Today.AddMonths(-1);
                return new DateTime(prev.Year, prev.Month, 1);
            }
        }

        public ManagerMeterForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += ManagerMeterForm_Load!;
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            MinimumSize = new Size(820, 480);
            ClientSize = new Size(1000, 650);
            BackColor = AppColors.Background;
            Text = "Ghi chỉ số Điện Nước";
            AutoScroll = false;

            pnlInput = new Panel { Dock = DockStyle.Right, Width = 350, MinimumSize = new Size(300, 0), BackColor = AppColors.Card, Padding = new Padding(20) };

            var lblTitle = new Label
            {
                Text = "Chốt số & Tạo hóa đơn",
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
                Location = new Point(20, 55),
                Size = new Size(300, 40)
            };

            lblBillingMonth = new Label
            {
                Text = $"Hóa đơn tháng: {BillingMonthStart:MM/yyyy}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.Success,
                Location = new Point(20, 95),
                Size = new Size(300, 22)
            };

            lblPrevMonth = new Label
            {
                Text = "Chỉ số kỳ trước: —",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                Location = new Point(20, 120),
                Size = new Size(300, 22)
            };
            lblOldElectric = new Label
            {
                Text = "Điện kỳ trước: —",
                ForeColor = AppColors.TextMuted,
                Location = new Point(20, 144),
                Size = new Size(300, 20)
            };
            lblOldWater = new Label
            {
                Text = "Nước kỳ trước: —",
                ForeColor = AppColors.TextMuted,
                Location = new Point(20, 166),
                Size = new Size(300, 20)
            };

            var lblElectric = new Label { Text = "Chỉ số điện MỚI *", Location = new Point(20, 200), AutoSize = true };
            txtElectric = new ModernTextBox { Location = new Point(20, 225), Size = new Size(300, 35) };
            var lblWater = new Label { Text = "Chỉ số nước MỚI *", Location = new Point(20, 270), AutoSize = true };
            txtWater = new ModernTextBox { Location = new Point(20, 295), Size = new Size(300, 35) };
            var lblFee = new Label { Text = "Phụ phí khác (nếu có)", Location = new Point(20, 340), AutoSize = true };
            txtFee = new ModernTextBox { Location = new Point(20, 365), Size = new Size(300, 35), Text = "0" };

            btnGenerateInvoice = new ModernButton
            {
                Text = $"Tạo hóa đơn {BillingMonthStart:MM/yyyy}",
                Location = new Point(20, 420),
                Size = new Size(300, 45),
                BackColor = AppColors.Primary,
                Enabled = false
            };
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click!;

            pnlInput.Controls.AddRange(new Control[]
            {
                lblTitle, lblSelectedRoom, lblBillingMonth, lblPrevMonth, lblOldElectric, lblOldWater,
                lblElectric, txtElectric, lblWater, txtWater, lblFee, txtFee, btnGenerateInvoice
            });
            foreach (Control c in pnlInput.Controls)
            {
                if (c is ModernTextBox or ModernButton)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                else if (c is Label { AutoSize: false })
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppColors.Background };
            pnlTop.Controls.Add(new Label
            {
                Text = "Hợp đồng Active (có khách) tại nhà được phân công",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Location = new Point(12, 14),
                AutoSize = true
            });
            var btnRefresh = new ModernButton
            {
                Text = "Làm mới",
                Size = new Size(100, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = AppColors.TextMuted
            };
            btnRefresh.Click += async (s, e) => await LoadActiveContractsAsync();
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Resize += (s, e) => btnRefresh.Location = new Point(Math.Max(12, pnlTop.ClientSize.Width - 112), 9);

            dgvContracts = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvContracts.AutoGenerateColumns = false;
            dgvContracts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã hợp đồng", FillWeight = 18 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 14 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", FillWeight = 28 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MonthlyRent", HeaderText = "Tiền thuê", FillWeight = 16 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 12 });
            dgvContracts.CellClick += DgvContracts_CellClick!;

            emptyState = new EmptyStatePanel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            pnlList = new Panel { Dock = DockStyle.Fill };
            pnlList.Controls.Add(dgvContracts);
            pnlList.Controls.Add(emptyState);
            pnlList.Controls.Add(pnlTop);

            Controls.Add(pnlList);
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
                using var scope = _scopeFactory.CreateScope();
                var contractService = scope.ServiceProvider.GetRequiredService<IContractService>();
                var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                int managerId = UserSession.CurrentUser!.UserID;

                var assignments = (await assignmentService.GetByManagerAsync(managerId))
                    .Where(a => string.Equals(a.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var contracts = (await contractService.GetContractsByManagerAsync(managerId)).ToList();
                var activeContracts = contracts
                    .Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) && c.TenantID.HasValue)
                    .ToList();

                dgvContracts.DataSource = null;
                dgvContracts.DataSource = activeContracts;

                if (activeContracts.Count == 0)
                {
                    dgvContracts.Visible = false;
                    if (assignments.Count == 0)
                    {
                        emptyState.ShowEmpty(
                            "Chưa được phân công nhà",
                            "Chủ nhà cần gán bạn (theo UserID) trong menu Phân công Manager trước khi ghi chỉ số.");
                    }
                    else if (contracts.Count == 0)
                    {
                        emptyState.ShowEmpty(
                            "Nhà đã gán chưa có hợp đồng",
                            $"Bạn đang quản lý {assignments.Count} nhà. Chủ nhà cần tạo hợp đồng Active có khách cho phòng thuộc nhà đó.");
                    }
                    else
                    {
                        emptyState.ShowEmpty(
                            "Chưa có hợp đồng Active có khách",
                            $"Có {contracts.Count} hợp đồng tại nhà được gán, nhưng chưa có HĐ Active + đã gán khách để ghi điện/nước.");
                    }
                }
                else
                {
                    emptyState.HideEmpty();
                    dgvContracts.Visible = true;
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách hợp đồng: " + ex.Message);
            }
        }

        private async void DgvContracts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var contract = dgvContracts.Rows[e.RowIndex].DataBoundItem as ContractDto;
            if (contract == null) return;

            _selectedContractId = contract.ContractID;
            lblSelectedRoom.Text = $"Phòng {contract.RoomNumber} - {contract.TenantName}";
            lblBillingMonth.Text = $"Hóa đơn tháng: {BillingMonthStart:MM/yyyy}";
            btnGenerateInvoice.Text = $"Tạo hóa đơn {BillingMonthStart:MM/yyyy}";
            btnGenerateInvoice.Enabled = true;
            await LoadPreviousReadingAsync(contract.ContractID);
        }

        private async Task LoadPreviousReadingAsync(int contractId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var last = await invoiceService.GetLatestReadingAsync(contractId);
                if (last == null)
                {
                    _prevElectric = 0;
                    _prevWater = 0;
                    lblPrevMonth.Text = "Chỉ số kỳ trước: chưa có (lần đầu)";
                    lblOldElectric.Text = "Điện kỳ trước: 0";
                    lblOldWater.Text = "Nước kỳ trước: 0";
                }
                else
                {
                    _prevElectric = last.NewElectric;
                    _prevWater = last.NewWater;
                    lblPrevMonth.Text = $"Chỉ số kỳ trước: {last.ReadingMonth:MM/yyyy}";
                    lblOldElectric.Text = $"Điện kỳ trước (đã chốt): {_prevElectric:N0}";
                    lblOldWater.Text = $"Nước kỳ trước (đã chốt): {_prevWater:N0}";
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không tải được chỉ số kỳ trước: " + ex.Message);
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

            if (newElectric < _prevElectric || newWater < _prevWater)
            {
                AppDialog.ShowWarning(
                    $"Chỉ số mới phải ≥ chỉ số kỳ trước.\n" +
                    $"Điện: ≥ {_prevElectric:N0}, Nước: ≥ {_prevWater:N0}");
                return;
            }

            btnGenerateInvoice.Enabled = false;
            try
            {
                var billingMonth = BillingMonthStart;
                var request = new GenerateInvoiceDto
                {
                    ContractID = _selectedContractId,
                    ReadingMonth = billingMonth,
                    NewElectric = newElectric,
                    NewWater = newWater,
                    OtherFee = otherFee,
                    CreatedBy = UserSession.CurrentUser!.UserID
                };
                using (var scope = _scopeFactory.CreateScope())
                {
                    var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                    await invoiceService.GenerateMonthlyInvoiceAsync(request);
                }
                AppDialog.ShowInfo($"Đã tạo hóa đơn tháng {billingMonth:MM/yyyy}. Khách thuê có thể xem và thanh toán.");
                txtElectric.Text = "";
                txtWater.Text = "";
                txtFee.Text = "0";
                lblSelectedRoom.Text = "Chưa chọn hợp đồng";
                lblPrevMonth.Text = "Chỉ số kỳ trước: —";
                lblOldElectric.Text = "Điện kỳ trước: —";
                lblOldWater.Text = "Nước kỳ trước: —";
                _selectedContractId = 0;
                _prevElectric = 0;
                _prevWater = 0;
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
                if (_selectedContractId > 0)
                    btnGenerateInvoice.Enabled = true;
            }
        }
    }
}
