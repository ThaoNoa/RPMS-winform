using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DTO.House;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace RPMS.WinForms.Forms.Landlord
{
    public partial class LandlordHouseForm : Form
    {
        private readonly IHouseService _houseService;
        private List<HouseDto> _houses;

        public LandlordHouseForm(IHouseService houseService)
        {
            InitializeComponent();
            _houseService = houseService;
            _houses = new List<HouseDto>();
            SetupDataGridView();
            this.Load += LandlordHouseForm_Load!;
        }

        private void SetupDataGridView()
        {
            dgvHouses.AutoGenerateColumns = false;
            dgvHouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseID", HeaderText = "ID", Width = 50 });
            dgvHouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Tên nhà", Width = 150 });
            dgvHouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Địa chỉ", Width = 250 });
            dgvHouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalRooms", HeaderText = "Số phòng", Width = 80 });
            dgvHouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgvHouses.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Sửa", UseColumnTextForLinkValue = true, Name = "EditCol", Width = 60 });
            dgvHouses.Columns.Add(new DataGridViewLinkColumn { HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, Name = "DeleteCol", Width = 60 });
        }

        private async void LandlordHouseForm_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var landlordId = UserSession.CurrentUser!.UserID;
                var result = await _houseService.GetHousesByOwnerAsync(landlordId);
                _houses = result.ToList();
                dgvHouses.DataSource = null;
                dgvHouses.DataSource = _houses;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var modal = Program.ServiceProvider.GetRequiredService<LandlordHouseModalForm>();
            modal.IsEditMode = false;
            if (modal.ShowDialog() == DialogResult.OK)
                await LoadDataAsync();
        }

        private async void dgvHouses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var house = dgvHouses.Rows[e.RowIndex].DataBoundItem as HouseDto;
            if (house == null) return;

            string colName = dgvHouses.Columns[e.ColumnIndex].Name;
            try
            {
                if (colName == "EditCol")
                {
                    var modal = Program.ServiceProvider.GetRequiredService<LandlordHouseModalForm>();
                    modal.IsEditMode = true;
                    modal.HouseIdToEdit = house.HouseID;
                    if (modal.ShowDialog() == DialogResult.OK)
                        await LoadDataAsync();
                }
                else if (colName == "DeleteCol")
                {
                    if (AppDialog.Confirm($"Bạn có chắc muốn xóa nhà '{house.HouseName}'? Yêu cầu nhà không còn phòng."))
                    {
                        await _houseService.DeleteHouseAsync(house.HouseID);
                        AppDialog.ShowInfo("Xóa nhà thành công.");
                        await LoadDataAsync();
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