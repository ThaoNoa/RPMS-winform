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
        private FlowLayoutPanel sideStack = null!;
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
            ClientSize = new Size(1000, 650);
            Text = "Ghi chỉ số Điện Nước";

            var btnRefresh = UIHelper.SecondaryButton("Làm mới", 100);
            btnRefresh.Click += async (s, e) => await LoadActiveContractsAsync();
            var header = UIHelper.CreatePageHeader(
                "Hợp đồng tại nhà được phân công (Active có khách → ghi điện/nước)",
                btnRefresh);

            pnlInput = UIHelper.CreateSideFormPanel();
            int fieldW = Math.Max(220, AppLayout.SidePanelWidth - AppLayout.PagePadding * 2 - 24);

            sideStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            sideStack.Controls.Add(new Label
            {
                Text = "Chốt số & Tạo hóa đơn",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, AppLayout.FieldGap)
            });

            lblSelectedRoom = new Label
            {
                Text = "Chưa chọn hợp đồng Active có khách",
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(fieldW, 0),
                Margin = new Padding(0, 0, 0, 6)
            };
            sideStack.Controls.Add(lblSelectedRoom);

            lblBillingMonth = new Label
            {
                Text = $"Hóa đơn tháng: {BillingMonthStart:MM/yyyy}",
                Font = AppTypography.BodyBold,
                ForeColor = AppColors.Success,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            sideStack.Controls.Add(lblBillingMonth);

            lblPrevMonth = new Label
            {
                Text = "Chỉ số kỳ trước: —",
                Font = AppTypography.BodyBold,
                ForeColor = AppColors.Primary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };
            sideStack.Controls.Add(lblPrevMonth);

            lblOldElectric = new Label
            {
                Text = "Điện kỳ trước: —",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };
            sideStack.Controls.Add(lblOldElectric);

            lblOldWater = new Label
            {
                Text = "Nước kỳ trước: —",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, AppLayout.FieldGap)
            };
            sideStack.Controls.Add(lblOldWater);

            txtElectric = new ModernTextBox();
            sideStack.Controls.Add(UIHelper.CreateLabeledField("Chỉ số điện MỚI *", txtElectric, fieldW));

            txtWater = new ModernTextBox();
            sideStack.Controls.Add(UIHelper.CreateLabeledField("Chỉ số nước MỚI *", txtWater, fieldW));

            txtFee = new ModernTextBox { Text = "0" };
            sideStack.Controls.Add(UIHelper.CreateLabeledField("Phụ phí khác (nếu có)", txtFee, fieldW));

            btnGenerateInvoice = UIHelper.PrimaryButton($"Tạo hóa đơn {BillingMonthStart:MM/yyyy}", fieldW);
            btnGenerateInvoice.Enabled = false;
            btnGenerateInvoice.Margin = new Padding(0, AppLayout.FieldGap, 0, 8);
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click!;
            sideStack.Controls.Add(btnGenerateInvoice);

            pnlInput.Controls.Add(sideStack);
            pnlInput.Resize += (_, _) => SyncSideFieldWidths();

            dgvContracts = new ModernDataGridView();
            UIHelper.ApplyGridFill(dgvContracts);
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 18 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", FillWeight = 16 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 12 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", FillWeight = 22 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MonthlyRent", HeaderText = "Tiền thuê", FillWeight = 14 });
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

            UIHelper.WirePage(this, pnlList, header, pnlInput);
            SyncSideFieldWidths();
        }

        private void SyncSideFieldWidths()
        {
            if (sideStack == null || pnlInput == null) return;
            int w = Math.Max(180, pnlInput.ClientSize.Width - pnlInput.Padding.Horizontal - 8);
            sideStack.Width = w;
            foreach (Control c in sideStack.Controls)
            {
                if (c is Panel field && field.Controls.Count >= 2)
                {
                    field.Width = w;
                    var input = field.Controls[1];
                    input.Width = w;
                }
                else if (c is ModernButton btn)
                {
                    btn.Width = w;
                }
                else if (c is Label lbl && lbl.MaximumSize.Width > 0)
                {
                    lbl.MaximumSize = new Size(w, 0);
                }
            }
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

                var contracts = (await contractService.GetContractsByManagerAsync(managerId))
                    .Where(c => !string.Equals(c.Status, "Terminated", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(c.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.HouseName)
                    .ThenBy(c => c.RoomNumber)
                    .ToList();

                // Ưu tiên hiện Active có khách trước; vẫn hiện Draft để manager thấy phòng được gán
                var displayContracts = contracts
                    .OrderByDescending(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) && c.TenantID.HasValue)
                    .ThenBy(c => c.HouseName)
                    .ThenBy(c => c.RoomNumber)
                    .ToList();

                dgvContracts.DataSource = null;
                dgvContracts.DataSource = displayContracts;

                if (displayContracts.Count == 0)
                {
                    dgvContracts.Visible = false;
                    if (assignments.Count == 0)
                    {
                        emptyState.ShowEmpty(
                            "Chưa được phân công nhà",
                            "Chủ nhà cần gán bạn (UserID / Username) trong menu Phân công Manager.");
                    }
                    else
                    {
                        emptyState.ShowEmpty(
                            "Nhà đã gán chưa có hợp đồng",
                            $"Bạn đang quản lý {assignments.Count} nhà. Chủ nhà cần tạo hợp đồng (Draft/Active) cho phòng thuộc nhà đó.");
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
            bool canBill = string.Equals(contract.Status, "Active", StringComparison.OrdinalIgnoreCase) && contract.TenantID.HasValue;
            lblSelectedRoom.Text = canBill
                ? $"{contract.HouseName} — Phòng {contract.RoomNumber} — {contract.TenantName}"
                : $"{contract.HouseName} — Phòng {contract.RoomNumber} — {contract.Status} (chưa ghi ĐN được)";
            lblBillingMonth.Text = $"Hóa đơn tháng: {BillingMonthStart:MM/yyyy}";
            btnGenerateInvoice.Text = $"Tạo hóa đơn {BillingMonthStart:MM/yyyy}";
            btnGenerateInvoice.Enabled = canBill;
            if (canBill)
                await LoadPreviousReadingAsync(contract.ContractID);
            else
            {
                _prevElectric = 0;
                _prevWater = 0;
                lblPrevMonth.Text = "Chỉ số kỳ trước: — (cần HĐ Active + khách)";
                lblOldElectric.Text = "Điện kỳ trước: —";
                lblOldWater.Text = "Nước kỳ trước: —";
            }
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
