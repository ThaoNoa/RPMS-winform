using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Maintenance;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Manager
{
    public class ManagerMaintenanceForm : Form
    {
        private readonly IMaintenanceService _maintenanceService;
        private ModernDataGridView dgvRequests = null!;

        public ManagerMaintenanceForm(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Quản lý Sự cố / Bảo trì";
            ClientSize = new Size(1100, 600);

            var btnRefresh = UIHelper.SecondaryButton("Làm mới", 110);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            var header = UIHelper.CreatePageHeader("Yêu cầu bảo trì từ Khách thuê", btnRefresh);

            dgvRequests = new ModernDataGridView();
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RequestID", HeaderText = "ID", FillWeight = 5 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 7 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", FillWeight = 12 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề sự cố", FillWeight = 18 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Ngày gửi",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 10 });
            dgvRequests.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DetailCol",
                HeaderText = "Chi tiết",
                Text = "Xem",
                UseColumnTextForLinkValue = true,
                FillWeight = 7,
                LinkColor = AppColors.Primary
            });
            dgvRequests.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "ProcessCol",
                HeaderText = "Tiếp nhận",
                Text = "Xác nhận & Hẹn",
                UseColumnTextForLinkValue = true,
                FillWeight = 11,
                LinkColor = Color.Blue
            });
            dgvRequests.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "CompleteCol",
                HeaderText = "Hoàn thành",
                Text = "Xong",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = Color.Green
            });
            dgvRequests.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "PrintCol",
                HeaderText = "In",
                Text = "In/PDF",
                UseColumnTextForLinkValue = true,
                FillWeight = 7,
                LinkColor = AppColors.Primary
            });
            dgvRequests.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DeleteCol",
                HeaderText = "Xóa",
                Text = "Xóa",
                UseColumnTextForLinkValue = true,
                FillWeight = 6,
                LinkColor = Color.Red
            });
            dgvRequests.CellContentClick += DgvRequests_CellContentClick!;
            dgvRequests.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvRequests.Rows[e.RowIndex].DataBoundItem is MaintenanceRequestDto req)
                    OpenDetail(req.RequestID);
            };

            Controls.Add(dgvRequests);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgvRequests);
            UIHelper.ApplyGridFill(dgvRequests);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var requests = await _maintenanceService.GetRequestsForManagerAsync(UserSession.CurrentUser!.UserID);
                dgvRequests.DataSource = requests.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu bảo trì: " + ex.Message);
            }
        }

        private void OpenDetail(int requestId)
        {
            try
            {
                var form = Program.ServiceProvider.GetRequiredService<MaintenanceDetailForm>();
                form.RequestId = requestId;
                form.ShowDialog(this);
                if (form.Changed)
                    _ = LoadDataAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không mở chi tiết sự cố: " + ex.Message);
            }
        }

        private async void DgvRequests_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var req = dgvRequests.Rows[e.RowIndex].DataBoundItem as MaintenanceRequestDto;
            if (req == null) return;
            string col = dgvRequests.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "DetailCol")
                {
                    OpenDetail(req.RequestID);
                }
                else if (col == "PrintCol")
                {
                    MaintenancePrintHelper.OpenAndPrint(req);
                }
                else if (col == "ProcessCol")
                {
                    if (req.Status == "Completed")
                    {
                        AppDialog.ShowInfo("Sự cố này đã hoàn thành.");
                        return;
                    }

                    var msg = AppDialog.Prompt(
                        "Nhập thông báo hẹn ngày sửa chữa để gửi cho Khách thuê:",
                        "Xác nhận Tiếp nhận Sự cố",
                        "Quản lý đã tiếp nhận và sẽ đến kiểm tra vào ngày mai.");

                    if (!string.IsNullOrEmpty(msg))
                    {
                        await _maintenanceService.UpdateRequestStatusAsync(req.RequestID, "Processing", UserSession.CurrentUser!.UserID);
                        await _maintenanceService.SendMaintenanceNotificationAsync(req.RequestID, msg);
                        AppDialog.ShowInfo("Đã xác nhận tiếp nhận và gửi thông báo cho khách thuê.");
                        await LoadDataAsync();
                    }
                }
                else if (col == "CompleteCol")
                {
                    if (req.Status == "Completed") return;
                    if (AppDialog.Confirm("Xác nhận sự cố đã được khắc phục hoàn tất?", "Hoàn thành"))
                    {
                        await _maintenanceService.UpdateRequestStatusAsync(req.RequestID, "Completed", UserSession.CurrentUser!.UserID);
                        await _maintenanceService.SendMaintenanceNotificationAsync(req.RequestID, "Sự cố của bạn đã được khắc phục hoàn tất.");
                        AppDialog.ShowInfo("Đã đánh dấu hoàn thành!");
                        await LoadDataAsync();
                    }
                }
                else if (col == "DeleteCol")
                {
                    if (AppDialog.Confirm("Bạn có chắc chắn muốn xóa vĩnh viễn yêu cầu này?", "Xóa"))
                    {
                        await _maintenanceService.DeleteRequestAsync(req.RequestID);
                        AppDialog.ShowInfo("Đã xóa yêu cầu bảo trì.");
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi xử lý: " + ex.Message);
            }
        }
    }
}
