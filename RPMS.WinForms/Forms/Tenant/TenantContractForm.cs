using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Contract;
using RPMS.WinForms.Controls;
using RPMS.WinForms.Forms.Shared;
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
            Activated += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Hợp đồng của tôi";
            ClientSize = new Size(1180, 620);

            var header = UIHelper.CreatePageHeader(
                "Hợp đồng — PendingConfirm: bấm Đồng ý / Từ chối (hoặc «Thông báo» → Xem chi tiết). Sửa·hủy duyệt trong Thông báo.");

            dgvContracts = new ModernDataGridView();
            dgvContracts.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvContracts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvContracts.RowTemplate.Height = 40;

            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", FillWeight = 12, MinimumWidth = 95
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 16, MinimumWidth = 100
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 8, MinimumWidth = 50
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Hết hạn",
                FillWeight = 11,
                MinimumWidth = 88,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status", HeaderText = "TT", FillWeight = 10, MinimumWidth = 72
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PendingEditStatus", HeaderText = "Sửa?", FillWeight = 7, MinimumWidth = 48
            });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CancelRequestLabel", HeaderText = "Xin hủy?", FillWeight = 9, MinimumWidth = 70
            });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DetailCol",
                HeaderText = "Chi tiết",
                Text = "Xem",
                UseColumnTextForLinkValue = true,
                FillWeight = 7,
                MinimumWidth = 52,
                LinkColor = AppColors.Primary
            });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "AcceptOfferCol",
                HeaderText = "Đồng ý",
                UseColumnTextForLinkValue = false,
                FillWeight = 8,
                MinimumWidth = 60,
                LinkColor = AppColors.Success
            });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "RejectOfferCol",
                HeaderText = "Từ chối",
                UseColumnTextForLinkValue = false,
                FillWeight = 8,
                MinimumWidth = 60,
                LinkColor = AppColors.Danger
            });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "ActionsCol",
                HeaderText = "Thao tác",
                Text = "⋯",
                UseColumnTextForLinkValue = true,
                FillWeight = 7,
                MinimumWidth = 52,
                LinkColor = AppColors.Primary
            });
            dgvContracts.CellFormatting += DgvContracts_CellFormatting!;
            dgvContracts.CellContentClick += DgvContracts_CellContentClick!;
            dgvContracts.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvContracts.Rows[e.RowIndex].DataBoundItem is ContractDto c)
                    await OpenDetailAsync(c.ContractID);
            };

            Controls.Add(dgvContracts);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgvContracts);
            UIHelper.ApplyGridFill(dgvContracts);
        }

        private static bool IsPendingConfirm(ContractDto? c) =>
            c != null && string.Equals(c.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase);

        private void DgvContracts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgvContracts.Columns[e.ColumnIndex].Name;
            if (col != "AcceptOfferCol" && col != "RejectOfferCol") return;

            var contract = dgvContracts.Rows[e.RowIndex].DataBoundItem as ContractDto;
            if (IsPendingConfirm(contract))
            {
                e.Value = col == "AcceptOfferCol" ? "Đồng ý" : "Từ chối";
                e.FormattingApplied = true;
            }
            else
            {
                e.Value = "";
                e.FormattingApplied = true;
            }
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

        private async System.Threading.Tasks.Task OpenDetailAsync(int contractId)
        {
            try
            {
                var detail = await _contractService.GetContractByIdAsync(contractId);
                using var dlg = new ContractDetailViewForm(detail);
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async System.Threading.Tasks.Task AcceptOfferAsync(ContractDto contract)
        {
            if (!IsPendingConfirm(contract))
            {
                AppDialog.ShowInfo("Chỉ xác nhận khi hợp đồng đang chờ (PendingConfirm).");
                return;
            }
            if (!AppDialog.Confirm(
                $"Đồng ý thuê phòng {contract.RoomNumber} ({contract.HouseName})?\n\nSau khi đồng ý, hợp đồng Active và phòng được đánh dấu đã thuê."))
                return;
            await _contractService.AcceptRentalOfferAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
            AppDialog.ShowInfo("Bạn đã đồng ý thuê. Hợp đồng đang Active.");
            ToastNotifier.Show(this, "Đã thuê thành công", ToastKind.Success);
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task RejectOfferAsync(ContractDto contract)
        {
            if (!IsPendingConfirm(contract))
            {
                AppDialog.ShowInfo("Chỉ từ chối khi hợp đồng đang chờ (PendingConfirm).");
                return;
            }
            if (!AppDialog.Confirm($"Từ chối đề nghị thuê phòng {contract.RoomNumber} ({contract.HouseName})?"))
                return;
            await _contractService.RejectRentalOfferAsync(contract.ContractID, UserSession.CurrentUser!.UserID);
            AppDialog.ShowInfo("Đã từ chối. Chủ nhà sẽ nhận thông báo.");
            ToastNotifier.Show(this, "Đã từ chối đề nghị", ToastKind.Info);
            await LoadDataAsync();
        }

        private async void DgvContracts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvContracts.Rows[e.RowIndex].DataBoundItem is not ContractDto contract) return;
            string col = dgvContracts.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "DetailCol")
                {
                    await OpenDetailAsync(contract.ContractID);
                    return;
                }

                if (col == "AcceptOfferCol")
                {
                    if (!IsPendingConfirm(contract)) return;
                    await AcceptOfferAsync(contract);
                    return;
                }

                if (col == "RejectOfferCol")
                {
                    if (!IsPendingConfirm(contract)) return;
                    await RejectOfferAsync(contract);
                    return;
                }

                if (col == "ActionsCol")
                {
                    ShowTenantActionsMenu(contract, e.RowIndex, e.ColumnIndex);
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private void ShowTenantActionsMenu(ContractDto contract, int rowIndex, int colIndex)
        {
            var menu = new ContextMenuStrip();
            bool isActive = string.Equals(contract.Status, "Active", StringComparison.OrdinalIgnoreCase);
            bool pendingConfirm = IsPendingConfirm(contract);
            bool pendingCancel = string.Equals(contract.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase);

            void Add(string text, Func<System.Threading.Tasks.Task> action, bool enabled = true)
            {
                var item = new ToolStripMenuItem(text) { Enabled = enabled };
                item.Click += async (_, _) =>
                {
                    try { await action(); }
                    catch (Exception ex) { AppDialog.ShowError(ex.Message); }
                };
                menu.Items.Add(item);
            }

            if (pendingConfirm)
            {
                Add("Đồng ý thuê", () => AcceptOfferAsync(contract));
                Add("Từ chối đề nghị", () => RejectOfferAsync(contract));
                menu.Items.Add(new ToolStripSeparator());
            }

            Add("In / PDF", async () =>
            {
                var detail = await _contractService.GetContractByIdAsync(contract.ContractID);
                ContractPrintHelper.OpenAndPrint(detail);
            });
            Add("Xin gia hạn", async () =>
            {
                if (!isActive)
                {
                    AppDialog.ShowInfo("Gia hạn chỉ dùng khi hợp đồng đang Active.");
                    return;
                }
                var reason = AppDialog.Prompt("Nhập lý do và thời gian muốn gia hạn thêm:", "Xin Gia Hạn Hợp Đồng", "");
                if (string.IsNullOrEmpty(reason)) return;
                await _tenantService.SendContractRequestAsync(UserSession.CurrentUser!.UserID, contract.ContractID, "Gia hạn", reason);
                AppDialog.ShowInfo("Đã gửi yêu cầu gia hạn cho chủ nhà.");
            }, enabled: isActive);
            Add("Xin hủy thuê", async () =>
            {
                if (!isActive)
                {
                    AppDialog.ShowInfo("Xin hủy chỉ dùng khi hợp đồng đang Active.");
                    return;
                }
                if (pendingCancel)
                {
                    AppDialog.ShowInfo(string.Equals(contract.CancelRequestedBy, "Tenant", StringComparison.OrdinalIgnoreCase)
                        ? "Bạn đã xin hủy — chủ duyệt/từ chối trong «Thông báo»."
                        : "Chủ đang xin hủy — mở «Thông báo» → Xem chi tiết để Duyệt/Từ chối.");
                    return;
                }
                var reason = AppDialog.Prompt("Nhập lý do và ngày dự kiến chuyển đi:", "Xin Hủy Thuê Phòng", "");
                if (string.IsNullOrWhiteSpace(reason)) return;
                if (!AppDialog.Confirm("Gửi yêu cầu hủy? Chủ nhà sẽ nhận Thông báo để phản hồi.")) return;
                await _contractService.RequestCancelAsync(contract.ContractID, UserSession.CurrentUser!.UserID, reason);
                AppDialog.ShowInfo("Đã gửi. Chủ mở «Thông báo» → Xem chi tiết. Sau khi xử lý, HĐ hai bên đồng bộ.");
                ToastNotifier.Show(this, "Đã xin hủy thuê", ToastKind.Info);
                await LoadDataAsync();
            }, enabled: isActive);

            if (pendingCancel)
            {
                menu.Items.Insert(0, new ToolStripMenuItem("Đang xin hủy — duyệt trong «Thông báo»") { Enabled = false });
                menu.Items.Insert(1, new ToolStripSeparator());
            }

            var rect = dgvContracts.GetCellDisplayRectangle(colIndex, rowIndex, true);
            menu.Show(dgvContracts, rect.Left, rect.Bottom);
        }
    }
}
