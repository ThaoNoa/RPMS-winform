using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DTO.Room;
using RPMS.WinForms.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace RPMS.WinForms.Forms.Landlord
{
    public partial class LandlordRoomForm : Form
    {
        private readonly IHouseService _houseService;
        private readonly IRoomService _roomService;

        public LandlordRoomForm(IHouseService houseService, IRoomService roomService)
        {
            InitializeComponent();
            _houseService = houseService;
            _roomService = roomService;
            SetupDataGridView();
            this.Load += LandlordRoomForm_Load!;
        }

        private void SetupDataGridView()
        {
            dgvRooms.AutoGenerateColumns = false;
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomID", HeaderText = "ID", Width = 50 });
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Mã Phòng", Width = 100 });
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Floor", HeaderText = "Tầng", Width = 50 });
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Area", HeaderText = "Diện tích (m2)", Width = 80 });
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Giá thuê", Width = 100 });
            dgvRooms.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgvRooms.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Chi tiết & Sửa", UseColumnTextForLinkValue = true, Name = "EditCol", Width = 80 });
            dgvRooms.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, Name = "DeleteCol", Width = 60 });
        }

        private async void LandlordRoomForm_Load(object sender, EventArgs e)
        {
            try
            {
                var landlordId = UserSession.CurrentUser!.UserID;
                var houses = await _houseService.GetHousesByOwnerAsync(landlordId);
                if (houses.Any())
                {
                    cboHouses.DataSource = houses.ToList();
                    cboHouses.DisplayMember = "HouseName";
                    cboHouses.ValueMember = "HouseID";
                }
                else
                {
                    btnAddRoom.Enabled = false;
                    AppDialog.ShowInfo("Bạn chưa có nhà nào. Vui lòng thêm nhà trước khi thêm phòng.");
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách nhà: " + ex.Message);
            }
        }

        private async void cboHouses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboHouses.SelectedValue != null && int.TryParse(cboHouses.SelectedValue.ToString(), out int houseId))
            {
                await LoadRoomsAsync(houseId);
            }
        }

        private async Task LoadRoomsAsync(int houseId)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByHouseAsync(houseId);
                dgvRooms.DataSource = null;
                dgvRooms.DataSource = rooms.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải danh sách phòng: " + ex.Message);
            }
        }

        private async void btnAddRoom_Click(object sender, EventArgs e)
        {
            if (cboHouses.SelectedValue == null) return;
            var modal = Program.ServiceProvider.GetRequiredService<LandlordRoomModalForm>();
            modal.IsEditMode = false;
            modal.HouseId = (int)cboHouses.SelectedValue;
            if (modal.ShowDialog() == DialogResult.OK)
                await LoadRoomsAsync(modal.HouseId);
        }

        private async void dgvRooms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var room = dgvRooms.Rows[e.RowIndex].DataBoundItem as RoomDto;
            if (room == null) return;

            string colName = dgvRooms.Columns[e.ColumnIndex].Name;
            try
            {
                if (colName == "EditCol")
                {
                    var modal = Program.ServiceProvider.GetRequiredService<LandlordRoomModalForm>();
                    modal.IsEditMode = true;
                    modal.HouseId = room.HouseID;
                    modal.RoomIdToEdit = room.RoomID;
                    if (modal.ShowDialog() == DialogResult.OK)
                        await LoadRoomsAsync(modal.HouseId);
                }
                else if (colName == "DeleteCol")
                {
                    if (AppDialog.Confirm($"Bạn có chắc muốn xóa phòng '{room.RoomNumber}'?"))
                    {
                        await _roomService.DeleteRoomAsync(room.RoomID);
                        AppDialog.ShowInfo("Xóa phòng thành công.");
                        if (cboHouses.SelectedValue != null)
                            await LoadRoomsAsync((int)cboHouses.SelectedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Thao tác thất bại: " + ex.Message);
            }
        }
    }
}