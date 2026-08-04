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
            Text = "Hợp đồng của tôi";
            ClientSize = new Size(1180, 620);

            var header = UIHelper.CreatePageHeader("Hợp đồng thuê phòng");

            dgvContracts = new ModernDataGridView();
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", FillWeight = 11 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 12 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 7 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Hết hạn",
                FillWeight = 9,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 10 });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "AcceptOfferCol",
                HeaderText = "",
                Text = "Đồng ý thuê",
                UseColumnTextForLinkValue = true,
                FillWeight = 10,
                LinkColor = AppColors.Success
            });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "RejectOfferCol",
                HeaderText = "",
                Text = "Từ chối thuê",
                UseColumnTextForLinkValue = true,
                FillWeight = 10,
                LinkColor = AppColors.Danger
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PendingEditStatus", HeaderText = "Sửa HĐ", FillWeight = 7 });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ViewPendingCol", HeaderText = "", Text = "Xem đề xuất", UseColumnTextForLinkValue = true, FillWeight = 9, LinkColor = AppColors.Primary });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ConfirmCol", HeaderText = "", Text = "Xác nhận sửa", UseColumnTextForLinkValue = true, FillWeight = 9, LinkColor = AppColors.Success });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "RejectCol", HeaderText = "", Text = "Từ chối sửa", UseColumnTextForLinkValue = true, FillWeight = 8, LinkColor = AppColors.Danger });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "PrintCol", HeaderText = "", Text = "In/PDF", UseColumnTextForLinkValue = true, FillWeight = 7, LinkColor = AppColors.Primary });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "ExtendCol", HeaderText = "", Text = "Xin gia hạn", UseColumnTextForLinkValue = true, FillWeight = 9, LinkColor = Color.Blue });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "CancelCol", HeaderText = "", Text = "Xin hủy thuê", UseColumnTextForLinkValue = true, FillWeight = 9, LinkColor = Color.Red });
            dgvContracts.CellContentClick += DgvContracts_CellContentClick!;

            Controls.Add(dgvContracts);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgvContracts);
            UIHelper.ApplyGridFill(dgvContracts);
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
                if (col == "AcceptOfferCol")
                {
                    if (!string.Equals(contract.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase))
                    {
                        AppDialog.ShowInfo("Chỉ xác nhận khi hợp đồng đang chờ (PendingConfirm).");
                        return;
                    }
                    if (!AppDialog.Confirm(
                        $"Đồng ý thuê phòng {contract.RoomNumber}?\n\nSau khi đồng ý, hợp đồng Active và phòng được đánh dấu đã thuê."))
                        return;
                    await _contractService.AcceptRentalOfferAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
                    AppDialog.ShowInfo("Bạn đã đồng ý thuê. Hợp đồng đang Active.");
                    ToastNotifier.Show(this, "Đã thuê thành công", ToastKind.Success);
                    await LoadDataAsync();
                    return;
                }

                if (col == "RejectOfferCol")
                {
                    if (!string.Equals(contract.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase))
                    {
                        AppDialog.ShowInfo("Chỉ từ chối khi hợp đồng đang chờ (PendingConfirm).");
                        return;
                    }
                    if (!AppDialog.Confirm($"Từ chối đề nghị thuê phòng {contract.RoomNumber}?"))
                        return;
                    await _contractService.RejectRentalOfferAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
                    AppDialog.ShowInfo("Đã từ chối. Chủ nhà sẽ nhận thông báo.");
                    await LoadDataAsync();
                    return;
                }

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
                    AppDialog.ShowInfo("Gia hạn / hủy thuê chỉ dùng khi hợp đồng đang Active.");
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
