using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DTO.House;
using RPMS.WinForms.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public partial class LandlordHouseModalForm : Form
    {
        private readonly IHouseService _houseService;
        public bool IsEditMode { get; set; }
        public int HouseIdToEdit { get; set; }

        public LandlordHouseModalForm(IHouseService houseService)
        {
            InitializeComponent();
            _houseService = houseService;
            this.Load += LandlordHouseModalForm_Load!;
        }

        private async void LandlordHouseModalForm_Load(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                lblTitle.Text = "Cập nhật thông tin Nhà";
                await LoadHouseDetailsAsync();
            }
            else
            {
                lblTitle.Text = "Thêm Nhà mới";
                cboStatus.SelectedIndex = 0;
                cboStatus.Enabled = false;
            }
            // Không SoftAnchor — layout TableLayout + CreateDialogField đã kéo giãn đúng,
            // SoftAnchor làm hỏng Dock của footer / ModernTextBox.
        }

        private async Task LoadHouseDetailsAsync()
        {
            try
            {
                var house = await _houseService.GetHouseByIdAsync(HouseIdToEdit);
                txtName.Text = house.HouseName;
                txtAddress.Text = house.Address;
                txtDescription.Text = house.Description;
                cboStatus.SelectedItem = house.Status;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải thông tin: " + ex.Message);
                this.Close();
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                AppDialog.ShowWarning("Vui lòng nhập tên nhà và địa chỉ.");
                return;
            }

            btnSave.Enabled = false;
            try
            {
                if (IsEditMode)
                {
                    var updateRequest = new UpdateHouseDto
                    {
                        HouseName = txtName.Text.Trim(),
                        Address = txtAddress.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Status = cboStatus.SelectedItem?.ToString() ?? "Active"
                    };
                    await _houseService.UpdateHouseAsync(HouseIdToEdit, updateRequest);
                }
                else
                {
                    var createRequest = new CreateHouseDto
                    {
                        OwnerID = UserSession.CurrentUser!.UserID,
                        HouseName = txtName.Text.Trim(),
                        Address = txtAddress.Text.Trim(),
                        Description = txtDescription.Text.Trim()
                    };
                    await _houseService.CreateHouseAsync(createRequest);
                }
                AppDialog.ShowInfo("Lưu thành công!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
