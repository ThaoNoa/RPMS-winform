using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Contract;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantContractForm : Form
    {
        private readonly IContractService _contractService;
        private readonly ITenantService _tenantService;
        private ModernDataGridView dgvContracts = null!;

        public TenantContractForm(IContractService contractService, ITenantService tenantService)
        {
            _contractService = contractService;
            _tenantService = tenantService;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            ClientSize = new Size(1100, 600);
            BackColor = AppColors.Background;
            Text = "Hợp đồng của tôi";

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Hợp đồng thuê phòng",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 18),
                AutoSize = true
            });

            dgvContracts = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvContracts.AutoGenerateColumns = false;
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã Hợp Đồng", Width = 140 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 80 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Ngày kết thúc",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 90 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PendingEditStatus", HeaderText = "Sửa HĐ", Width = 80 });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ViewPendingCol", HeaderText = "", Text = "Xem đề xuất", UseColumnTextForLinkValue = true, Width = 100, LinkColor = AppColors.Primary });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ConfirmCol", HeaderText = "", Text = "Xác nhận sửa", UseColumnTextForLinkValue = true, Width = 100, LinkColor = AppColors.Success });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "RejectCol", HeaderText = "", Text = "Từ chối", UseColumnTextForLinkValue = true, Width = 80, LinkColor = AppColors.Danger });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "PrintCol", HeaderText = "", Text = "In/PDF", UseColumnTextForLinkValue = true, Width = 70, LinkColor = AppColors.Primary });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ExtendCol", HeaderText = "Gia hạn", Text = "Xin Gia hạn", UseColumnTextForLinkValue = true, Width = 100, LinkColor = Color.Blue });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "CancelCol", HeaderText = "Hủy thuê", Text = "Xin Hủy thuê", UseColumnTextForLinkValue = true, Width = 100, LinkColor = Color.Red });
            dgvContracts.CellContentClick += DgvContracts_CellContentClick!;

            Controls.Add(dgvContracts);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgvContracts);
            MinimumSize = new Size(700, 480);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
                if (IsDisposed) return;
                dgvContracts.DataSource = contracts.ToList();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải được hợp đồng: " + ex.Message);
            }
        }

        private async void DgvContracts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvContracts.Rows[e.RowIndex].DataBoundItem is not ContractDto contract) return;
            string col = dgvContracts.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "PrintCol")
                {
                    var detail = await _contractService.GetContractByIdAsync(contract.ContractID);
                    ContractPrintHelper.OpenAndPrint(detail);
                    return;
                }

                if (col == "ViewPendingCol" || col == "ConfirmCol" || col == "RejectCol")
                {
                    if (!string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        AppDialog.ShowInfo("Không có đề xuất sửa đang chờ xác nhận.");
                        return;
                    }

                    var detail = await _contractService.GetContractByIdAsync(contract.ContractID);
                    if (col == "ViewPendingCol")
                    {
                        AppDialog.ShowInfo(
                            $"Đề xuất sửa {detail.ContractCode}:\n\n" +
                            $"Tiền thuê: {detail.MonthlyRent:N0} → {detail.PendingMonthlyRent:N0} đ\n" +
                            $"Giá điện: {detail.ElectricPrice:N0} → {detail.PendingElectricPrice:N0}\n" +
                            $"Giá nước: {detail.WaterPrice:N0} → {detail.PendingWaterPrice:N0}\n" +
                            $"Cọc: {detail.Deposit:N0} → {detail.PendingDeposit:N0}\n" +
                            $"Hết hạn: {detail.EndDate:dd/MM/yyyy} → {detail.PendingEndDate:dd/MM/yyyy}\n" +
                            $"Ghi chú: {detail.PendingEditNote ?? "(không)"}\n\n" +
                            "Sau khi xác nhận, giá mới áp dụng từ ngày xác nhận; ngày trước đó tính giá cũ.");
                        return;
                    }

                    if (col == "ConfirmCol")
                    {
                        if (!AppDialog.Confirm("Xác nhận áp dụng thay đổi hợp đồng? Giá mới có hiệu lực từ hôm nay."))
                            return;
                        await _contractService.ConfirmContractEditAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
                        AppDialog.ShowInfo("Đã xác nhận. Hợp đồng đã cập nhật chính thức.");
                        ToastNotifier.Show(this, "Đã xác nhận sửa HĐ", ToastKind.Success);
                        await LoadDataAsync();
                        return;
                    }

                    if (col == "RejectCol")
                    {
                        if (!AppDialog.Confirm("Từ chối đề xuất sửa hợp đồng?"))
                            return;
                        await _contractService.RejectContractEditAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
                        AppDialog.ShowInfo("Đã từ chối đề xuất sửa.");
                        await LoadDataAsync();
                        return;
                    }
                }

                if (contract.Status != "Active")
                {
                    AppDialog.ShowInfo("Chỉ có thể thao tác với hợp đồng đang Active.");
                    return;
                }

                if (col == "ExtendCol")
                {
                    var reason = AppDialog.Prompt("Nhập lý do và thời gian muốn gia hạn thêm:", "Xin Gia Hạn Hợp Đồng", "");
                    if (!string.IsNullOrEmpty(reason))
                    {
                        await _tenantService.SendContractRequestAsync(UserSession.CurrentUser!.UserID, contract.ContractID, "Gia hạn", reason);
                        AppDialog.ShowInfo("Đã gửi yêu cầu gia hạn cho chủ nhà.");
                    }
                }
                else if (col == "CancelCol")
                {
                    var reason = AppDialog.Prompt("Nhập lý do và ngày dự kiến chuyển đi:", "Xin Hủy Thuê Phòng", "");
                    if (!string.IsNullOrEmpty(reason))
                    {
                        await _tenantService.SendContractRequestAsync(UserSession.CurrentUser!.UserID, contract.ContractID, "Hủy thuê", reason);
                        AppDialog.ShowInfo("Đã gửi yêu cầu hủy thuê cho chủ nhà. Vui lòng chờ phản hồi.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
