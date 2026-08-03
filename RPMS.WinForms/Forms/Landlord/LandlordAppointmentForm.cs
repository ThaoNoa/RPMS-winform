using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Tenant;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordAppointmentForm : Form
    {
        private readonly ILandlordService _landlordService;
        private readonly IHouseService _houseService;
        private ComboBox cboHouse, cboStatus;
        private DateTimePicker dtpFrom, dtpTo;
        private ModernButton btnFilter;
        private ModernDataGridView dgvAppointments;

        public LandlordAppointmentForm(ILandlordService landlordService, IHouseService houseService)
        {
            _landlordService = landlordService;
            _houseService = houseService;
            InitializeUI();
            this.Load += LandlordAppointmentForm_Load;
        }

        private void InitializeUI()
        {
            this.ClientSize = new Size(1050, 650);
            this.BackColor = AppColors.Background;
            this.Text = "Quản lý Lịch hẹn Xem phòng";

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppColors.Card, Padding = new Padding(10) };
            Label lblHouse = new Label { Text = "Tòa nhà:", Location = new Point(20, 25), AutoSize = true };
            cboHouse = new ComboBox { Location = new Point(80, 22), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblStatus = new Label { Text = "Trạng thái:", Location = new Point(300, 25), AutoSize = true };
            cboStatus = new ComboBox { Location = new Point(380, 22), Size = new Size(150, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "All", "Pending", "Accepted", "Completed", "Rejected" });
            cboStatus.SelectedIndex = 0;

            Label lblDate = new Label { Text = "Khoảng 10 ngày từ:", Location = new Point(20, 60), AutoSize = true };
            dtpFrom = new DateTimePicker { Location = new Point(150, 58), Size = new Size(130, 30), Format = DateTimePickerFormat.Short };
            Label lblTo = new Label { Text = "đến:", Location = new Point(290, 60), AutoSize = true };
            dtpTo = new DateTimePicker { Location = new Point(330, 58), Size = new Size(130, 30), Format = DateTimePickerFormat.Short };
            dtpFrom.Value = DateTime.Now.AddDays(-2);
            dtpTo.Value = DateTime.Now.AddDays(8);

            btnFilter = new ModernButton { Text = "Lọc dữ liệu", Location = new Point(550, 20), Size = new Size(120, 40), BackColor = AppColors.Primary };
            btnFilter.Click += async (s, e) => await LoadDataAsync();

            pnlTop.Controls.AddRange(new Control[] { lblHouse, cboHouse, lblStatus, cboStatus, lblDate, dtpFrom, lblTo, dtpTo, btnFilter });

            dgvAppointments = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentID", HeaderText = "ID", Width = 50 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentDate", HeaderText = "Thời gian hẹn", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Note", HeaderText = "Ghi chú của khách", Width = 200 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn { Name = "AcceptCol", HeaderText = "Xác nhận", Text = "Nhận", UseColumnTextForLinkValue = true, Width = 80, LinkColor = Color.Blue });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn { Name = "CompleteCol", HeaderText = "Hoàn thành", Text = "Hoàn thành", UseColumnTextForLinkValue = true, Width = 90, LinkColor = Color.Green });
            dgvAppointments.Columns.Add(new DataGridViewLinkColumn { Name = "RejectCol", HeaderText = "Từ chối", Text = "Từ chối", UseColumnTextForLinkValue = true, Width = 80, LinkColor = Color.Red });
            dgvAppointments.CellContentClick += DgvAppointments_CellContentClick;

            this.Controls.Add(dgvAppointments);
            this.Controls.Add(pnlTop);
        }

        private async void LandlordAppointmentForm_Load(object sender, EventArgs e)
        {
            var houses = await _houseService.GetHousesByOwnerAsync(UserSession.CurrentUser!.UserID);
            var houseList = houses.ToList();
            houseList.Insert(0, new RPMS.DTO.House.HouseDto { HouseID = 0, HouseName = "Tất cả tòa nhà" });
            cboHouse.DataSource = houseList;
            cboHouse.DisplayMember = "HouseName";
            cboHouse.ValueMember = "HouseID";
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            int? houseId = (int)cboHouse.SelectedValue == 0 ? null : (int)cboHouse.SelectedValue;
            var data = await _landlordService.GetAppointmentsAsync(
                UserSession.CurrentUser!.UserID, houseId, cboStatus.SelectedItem.ToString(), dtpFrom.Value, dtpTo.Value);
            dgvAppointments.DataSource = data.ToList();
        }

        private async void DgvAppointments_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var app = dgvAppointments.Rows[e.RowIndex].DataBoundItem as AppointmentDto;
            string colName = dgvAppointments.Columns[e.ColumnIndex].Name;
            try
            {
                string newStatus = "";
                if (colName == "AcceptCol") newStatus = "Accepted";
                else if (colName == "CompleteCol") newStatus = "Completed";
                else if (colName == "RejectCol") newStatus = "Rejected";
                if (!string.IsNullOrEmpty(newStatus))
                {
                    await _landlordService.UpdateAppointmentStatusAsync(app.AppointmentID, newStatus);
                    AppDialog.ShowInfo("Cập nhật trạng thái thành công!");
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi: " + ex.Message);
            }
        }
    }
}