using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Notification;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class NotificationCenterForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgv = null!;
        private ModernTextBox txtSearch = null!;
        private ComboBox cboFilter = null!;
        private Label lblUnread = null!;

        public NotificationCenterForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
            Activated += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Trung tâm thông báo";
            ClientSize = new Size(1100, 620);

            var btnRefresh = UIHelper.SecondaryButton("Làm mới", 110);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var btnMarkAll = UIHelper.PrimaryButton("Đọc tất cả", 120);
            btnMarkAll.BackColor = AppColors.Success;
            btnMarkAll.Click += async (s, e) =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<INotificationService>()
                        .MarkAllAsReadAsync(UserSession.CurrentUser!.UserID);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError("Không đánh dấu đọc: " + ex.Message);
                }
            };

            var header = UIHelper.CreatePageHeader("Thông báo", btnRefresh, btnMarkAll);
            lblUnread = UIHelper.GetPageHeaderTitle(header);

            txtSearch = new ModernTextBox { PlaceholderText = "Tìm tiêu đề / nội dung" };
            cboFilter = new ComboBox();
            UIHelper.StyleCombo(cboFilter);
            cboFilter.Items.AddRange(new object[] { "Tất cả", "Chưa đọc", "Đã đọc", "Cần xử lý" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            txtSearch.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await LoadDataAsync();
                }
            };

            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Tìm kiếm", txtSearch, 260));
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Lọc", cboFilter, 140));

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", FillWeight = 22 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Content", HeaderText = "Nội dung", FillWeight = 34 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionLabel", HeaderText = "Loại", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionStatus", HeaderText = "TT xử lý", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsRead", HeaderText = "Đọc", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "ViewCol",
                HeaderText = "",
                Text = "Xem chi tiết",
                UseColumnTextForLinkValue = true,
                FillWeight = 10,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DeleteCol",
                HeaderText = "",
                Text = "Xóa",
                UseColumnTextForLinkValue = true,
                FillWeight = 6,
                LinkColor = AppColors.Danger
            });
            dgv.CellContentClick += Dgv_CellContentClick!;
            dgv.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                await OpenDetailAsync(dgv.Rows[e.RowIndex].DataBoundItem as NotificationRow);
            };

            Controls.Add(dgv);
            Controls.Add(filterBar);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                bool? isRead = cboFilter.SelectedIndex switch
                {
                    1 => false,
                    2 => true,
                    _ => null
                };

                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var list = (await svc.GetByUserAsync(
                    UserSession.CurrentUser!.UserID,
                    isRead,
                    txtSearch.Text)).ToList();

                if (cboFilter.SelectedIndex == 3)
                    list = list.Where(n => n.CanAct).ToList();

                if (IsDisposed) return;
                dgv.DataSource = list.Select(n => new NotificationRow
                {
                    NotificationID = n.NotificationID,
                    Title = n.Title,
                    Content = n.Content,
                    ActionType = n.ActionType,
                    RelatedID = n.RelatedID,
                    ActionStatus = n.ActionStatus,
                    ActionLabel = ActionLabel(n.ActionType),
                    CanAct = n.CanAct,
                    IsRead = n.IsRead ? "Rồi" : "Chưa",
                    CreatedDate = n.CreatedDate
                }).ToList();

                int unread = await svc.GetUnreadCountAsync(UserSession.CurrentUser.UserID);
                if (IsDisposed) return;
                int needAct = list.Count(n => n.CanAct);
                lblUnread.Text = needAct > 0
                    ? $"Thông báo ({unread} chưa đọc · {needAct} cần xử lý)"
                    : $"Thông báo ({unread} chưa đọc)";
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải thông báo: " + ex.Message);
            }
        }

        private static string ActionLabel(string? t) => t switch
        {
            NotificationActions.ContractEdit => "Sửa HĐ",
            NotificationActions.ContractCancel => "Hủy thuê",
            NotificationActions.ContractConfirm => "Xác nhận thuê",
            _ => ""
        };

        private async void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as NotificationRow;
            if (item == null) return;
            string col = dgv.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "ViewCol")
                {
                    await OpenDetailAsync(item);
                }
                else if (col == "DeleteCol")
                {
                    if (!AppDialog.Confirm("Xóa thông báo này?")) return;
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<INotificationService>()
                        .DeleteAsync(item.NotificationID);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task OpenDetailAsync(NotificationRow? item)
        {
            if (item == null) return;
            using var scope = _scopeFactory.CreateScope();
            var notifSvc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var contracts = scope.ServiceProvider.GetRequiredService<IContractService>();

            if (item.IsRead == "Chưa")
                await notifSvc.MarkAsReadAsync(item.NotificationID);

            string extra = "";
            if (item.RelatedID is > 0)
            {
                try
                {
                    var d = await contracts.GetContractByIdAsync(item.RelatedID.Value);
                    var sb = new StringBuilder();
                    sb.AppendLine("--- Hợp đồng liên quan ---");
                    sb.AppendLine($"{d.ContractCode} · {d.Status} · Phòng {d.RoomNumber}");
                    sb.AppendLine($"Thuê {d.MonthlyRent:N0}đ · Đến {d.EndDate:dd/MM/yyyy}");
                    if (string.Equals(d.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"Đề xuất sửa: thuê {d.PendingMonthlyRent:N0}, điện {d.PendingElectricPrice:N0}, nước {d.PendingWaterPrice:N0}, hết hạn {d.PendingEndDate:dd/MM/yyyy}");
                        if (!string.IsNullOrWhiteSpace(d.PendingEditNote))
                            sb.AppendLine($"Ghi chú: {d.PendingEditNote}");
                    }
                    if (string.Equals(d.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                        sb.AppendLine($"Xin hủy ({d.CancelRequestLabel}): {d.CancelRequestNote}");
                    extra = sb.ToString();
                }
                catch
                {
                    extra = $"(Không tải được HĐ #{item.RelatedID})";
                }
            }

            // Refresh CanAct from live DTO
            var live = await notifSvc.GetByIdAsync(item.NotificationID);
            bool canAct = live?.CanAct == true;
            // Also require contract still pending
            if (canAct && item.RelatedID is > 0)
            {
                try
                {
                    var d = await contracts.GetContractByIdAsync(item.RelatedID.Value);
                    if (item.ActionType == NotificationActions.ContractEdit)
                        canAct = string.Equals(d.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase);
                    else if (item.ActionType == NotificationActions.ContractCancel)
                        canAct = string.Equals(d.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase)
                                 && d.Status == "Active";
                    else if (item.ActionType == NotificationActions.ContractConfirm)
                        canAct = string.Equals(d.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase);
                }
                catch { canAct = false; }
            }

            using var dlg = new NotificationActionForm(contracts, new NotificationDtoWrap
            {
                NotificationID = item.NotificationID,
                Title = item.Title,
                Content = item.Content,
                CreatedDate = item.CreatedDate,
                ActionType = item.ActionType,
                RelatedID = item.RelatedID,
                ActionStatus = item.ActionStatus,
                CanAct = canAct
            }, extra);
            dlg.ShowDialog(this);
            await LoadDataAsync();
        }

        private sealed class NotificationRow
        {
            public int NotificationID { get; set; }
            public string Title { get; set; } = "";
            public string Content { get; set; } = "";
            public string? ActionType { get; set; }
            public int? RelatedID { get; set; }
            public string? ActionStatus { get; set; }
            public string ActionLabel { get; set; } = "";
            public bool CanAct { get; set; }
            public string IsRead { get; set; } = "";
            public DateTime CreatedDate { get; set; }
        }
    }
}
