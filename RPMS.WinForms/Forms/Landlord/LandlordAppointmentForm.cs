using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Tenant;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordAppointmentForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ComboBox cboHouse = null!;
        private ComboBox cboStatus = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private ModernButton btnFilter = null!;
        private ModernDataGridView dgvAppointments = null!;

        public LandlordAppointmentForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += LandlordAppointmentForm_Load!;
        }

        private void InitializeUI()
        {
            Text = "Quản lý Lịch hẹn Xem phòng";
            ClientSize = new Size(1100, 650);

            var header = UIHelper.CreatePageHeader("Quản lý lịch hẹn xem phòng");

            cboHouse = new ComboBox();
            UIHelper.StyleCombo(cboHouse);

            cboStatus = new ComboBox();
            UIHelper.StyleCombo(cboStatus);
            cboStatus.Items.AddRange(new object[] { "All", "Pending", "Accepted", "Completed", "Rejected" });
            cboStatus.SelectedIndex = 0;

            dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short };
            dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short };
            dtpFrom.Value = DateTime.Now.AddDays(-2);
            dtpTo.Value = DateTime.Now.AddDays(8);

            btnFilter = UIHelper.PrimaryButton("Lọc dữ liệu", 130);
            btnFilter.Margin = new Padding(0, 18, AppLayout.FieldGap, 6);
            btnFilter.Click += async (s, e) => await LoadDataAsync();

            var lblHint = new Label
            {
                Text = "Xác nhận / từ chối sẽ gửi thông báo ngay cho khách thuê.",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 28, 0, 6)
            };

            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Tòa nhà", cboHouse, 220));
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Trạng thái", cboStatus, 150));
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Từ ngày", dtpFrom, 130));
            filterBar.Controls.Add(UIHelper.CreateLabeledField("đến", dtpTo, 130));
            filterBar.Controls.Add(btnFilter);
            filterBar.Controls.Add(lblHint);

            dgvAppointments = new ModernDataGridView();
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentID", HeaderText = "ID", FillWeight = 6 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 10 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", FillWeight = 16 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AppointmentDate",
                HeaderText = "Thời gian hẹn",
                FillWeight = 16,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Note", HeaderText = "Ghi chú", FillWeight = 18 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 10 });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "AcceptCol",
                HeaderText = "Xác nhận",
                Text = "Nhận",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = Color.Blue
            });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "CompleteCol",
                HeaderText = "Hoàn thành",
                Text = "Xong",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = Color.Green
            });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "RejectCol",
                HeaderText = "Từ chối",
                Text = "Từ chối",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = Color.Red
            });
            dgvAppointments.CellContentClick += DgvAppointments_CellContentClick!;

            Controls.Add(dgvAppointments);
            Controls.Add(filterBar);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgvAppointments);
            UIHelper.ApplyGridFill(dgvAppointments);
        }

        private async void LandlordAppointmentForm_Load(object? sender, EventArgs e)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var houses = await scope.ServiceProvider.GetRequiredService<IHouseService>()
                    .GetHousesByOwnerAsync(UserSession.CurrentUser!.UserID);
                if (IsDisposed) return;
                var houseList = houses.ToList();
                houseList.Insert(0, new RPMS.DTO.House.HouseDto { HouseID = 0, HouseName = "Tất cả tòa nhà" });
                cboHouse.DisplayMember = "HouseName";
                cboHouse.ValueMember = "HouseID";
                cboHouse.DataSource = houseList;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Lỗi tải lịch hẹn: " + ex.Message);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (cboHouse.SelectedValue == null) return;
                int selectedHouse = Convert.ToInt32(cboHouse.SelectedValue);
                int? houseId = selectedHouse == 0 ? null : selectedHouse;
                string status = cboStatus.SelectedItem?.ToString() ?? "All";

                using var scope = _scopeFactory.CreateScope();
                var data = await scope.ServiceProvider.GetRequiredService<ILandlordService>()
                    .GetAppointmentsAsync(UserSession.CurrentUser!.UserID, houseId, status, dtpFrom.Value, dtpTo.Value);
                if (IsDisposed) return;
                dgvAppointments.DataSource = data.ToList();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Lỗi lọc lịch hẹn: " + ex.Message);
            }
        }

        private async void DgvAppointments_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var app = dgvAppointments.Rows[e.RowIndex].DataBoundItem as AppointmentDto;
            if (app == null) return;
            string colName = dgvAppointments.Columns[e.ColumnIndex].Name;

            string newStatus = colName switch
            {
                "AcceptCol" => "Accepted",
                "CompleteCol" => "Completed",
                "RejectCol" => "Rejected",
                _ => ""
            };
            if (string.IsNullOrEmpty(newStatus)) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ILandlordService>()
                    .UpdateAppointmentStatusAsync(app.AppointmentID, newStatus);

                string msg = newStatus switch
                {
                    "Accepted" => "Đã xác nhận lịch hẹn. Khách thuê đã nhận thông báo.",
                    "Rejected" => "Đã từ chối lịch hẹn. Khách thuê đã nhận thông báo.",
                    "Completed" => "Đã đánh dấu hoàn thành. Khách thuê đã nhận thông báo.",
                    _ => "Cập nhật thành công."
                };
                ToastNotifier.Show(this, msg, ToastKind.Success);
                AppDialog.ShowInfo(msg);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi: " + ex.Message);
            }
        }
    }
}
