using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DTO.Amenity;
using RPMS.DTO.Room;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public partial class LandlordRoomModalForm : Form
    {
        private readonly IRoomService _roomService;
        private readonly IAmenityService _amenityService;
        public bool IsEditMode { get; set; }
        public int HouseId { get; set; }
        public int RoomIdToEdit { get; set; }
        private List<string> _tempImagePaths = new List<string>();

        public LandlordRoomModalForm(IRoomService roomService, IAmenityService amenityService)
        {
            InitializeComponent();
            _roomService = roomService;
            _amenityService = amenityService;
            this.Load += LandlordRoomModalForm_Load!;
        }

        private async void LandlordRoomModalForm_Load(object sender, EventArgs e)
        {
            await LoadAmenitiesAsync();
            if (IsEditMode)
            {
                lblTitle.Text = "Cập nhật thông tin Phòng";
                await LoadRoomDetailsAsync();
            }
            else
            {
                lblTitle.Text = "Thêm Phòng mới";
                cboStatus.SelectedIndex = 0;
                cboStatus.Enabled = false;
                txtCapacity.Text = "1";
                txtBedroom.Text = "0";
                txtBathroom.Text = "0";
            }
            UIHelper.SoftAnchorDialogControls(this);
        }

        private async Task LoadAmenitiesAsync()
        {
            try
            {
                var amenities = await _amenityService.GetAllAmenitiesAsync();
                clbAmenities.DataSource = amenities.ToList();
                clbAmenities.DisplayMember = "AmenityName";
                clbAmenities.ValueMember = "AmenityID";
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải tiện ích: " + ex.Message);
            }
        }

        private async Task LoadRoomDetailsAsync()
        {
            try
            {
                var room = await _roomService.GetRoomDetailAsync(RoomIdToEdit);
                txtRoomNumber.Text = room.RoomNumber;
                txtFloor.Text = room.Floor.ToString();
                txtArea.Text = room.Area.ToString("0.##");
                txtPrice.Text = room.Price.ToString("0.##");
                txtCapacity.Text = room.Capacity.ToString();
                txtBedroom.Text = room.Bedroom.ToString();
                txtBathroom.Text = room.Bathroom.ToString();
                cboStatus.SelectedItem = room.Status;
                txtFurniture.Text = room.Furniture;
                txtDescription.Text = room.Description;

                var roomAmenityIds = room.Amenities.Select(a => a.AmenityID).ToList();
                for (int i = 0; i < clbAmenities.Items.Count; i++)
                {
                    var item = clbAmenities.Items[i] as AmenityDto;
                    if (item != null && roomAmenityIds.Contains(item.AmenityID))
                        clbAmenities.SetItemChecked(i, true);
                }

                _tempImagePaths = room.Images.ToList();
                UpdateImagesList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải thông tin phòng: " + ex.Message);
                this.Close();
            }
        }

        private void btnAddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter = "Ảnh & Video|*.jpg;*.jpeg;*.png;*.bmp;*.webp;*.mp4;*.webm;*.avi;*.mov;*.mkv;*.wmv|Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.webp|Video|*.mp4;*.webm;*.avi;*.mov;*.mkv;*.wmv";
                ofd.Title = "Chọn ảnh / video phòng";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                        _tempImagePaths.Add(file);
                    UpdateImagesList();
                    ToastNotifier.Show(this, $"Đã thêm {ofd.FileNames.Length} tệp", ToastKind.Success);
                }
            }
        }

        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            if (lstImages.SelectedIndex >= 0)
            {
                _tempImagePaths.RemoveAt(lstImages.SelectedIndex);
                UpdateImagesList();
                picPreview.Image = null;
            }
        }

        private void UpdateImagesList()
        {
            lstImages.Items.Clear();
            foreach (var path in _tempImagePaths)
            {
                var name = Path.GetFileName(path);
                if (ImagePathHelper.IsVideo(path))
                    name = "🎬 " + name;
                lstImages.Items.Add(name);
            }
        }

        private void lstImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstImages.SelectedIndex >= 0)
            {
                string path = _tempImagePaths[lstImages.SelectedIndex];
                ImagePathHelper.ApplyToPictureBox(picPreview, path);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text) ||
                !decimal.TryParse(txtArea.Text, out decimal area) || area <= 0 ||
                !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0 ||
                !int.TryParse(txtCapacity.Text, out int capacity) || capacity < 1 ||
                !int.TryParse(txtBedroom.Text, out int bed) || bed < 0 ||
                !int.TryParse(txtBathroom.Text, out int bath) || bath < 0)
            {
                AppDialog.ShowWarning("Vui lòng nhập đúng các trường số (diện tích, giá, sức chứa, số phòng).");
                return;
            }

            btnSave.Enabled = false;
            int roomId = 0;
            try
            {
                if (IsEditMode)
                {
                    var updateRequest = new UpdateRoomDto
                    {
                        RoomNumber = txtRoomNumber.Text.Trim(),
                        Floor = int.TryParse(txtFloor.Text, out int floor) ? floor : 0,
                        Area = area,
                        Price = price,
                        Capacity = capacity,
                        Bedroom = bed,
                        Bathroom = bath,
                        Furniture = txtFurniture.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Status = cboStatus.SelectedItem.ToString()
                    };
                    await _roomService.UpdateRoomAsync(RoomIdToEdit, updateRequest);
                    roomId = RoomIdToEdit;
                }
                else
                {
                    var createRequest = new CreateRoomDto
                    {
                        HouseID = HouseId,
                        RoomNumber = txtRoomNumber.Text.Trim(),
                        Floor = int.TryParse(txtFloor.Text, out int floor) ? floor : 0,
                        Area = area,
                        Price = price,
                        Capacity = capacity,
                        Bedroom = bed,
                        Bathroom = bath,
                        Furniture = txtFurniture.Text.Trim(),
                        Description = txtDescription.Text.Trim()
                    };
                    var createdRoom = await _roomService.CreateRoomAsync(createRequest);
                    roomId = createdRoom.RoomID;
                }

                var selectedAmenities = new List<int>();
                foreach (var item in clbAmenities.CheckedItems)
                {
                    if (item is AmenityDto amenity)
                        selectedAmenities.Add(amenity.AmenityID);
                }
                await _roomService.AssignAmenitiesAsync(roomId, selectedAmenities);

                var finalImagePaths = new List<string>();
                string uploadFolder = Path.Combine(Application.StartupPath, "uploads", "rooms");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var originalPath in _tempImagePaths)
                {
                    if (originalPath.StartsWith("/"))
                    {
                        finalImagePaths.Add(originalPath);
                    }
                    else if (File.Exists(originalPath))
                    {
                        string fileName = $"room_{roomId}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(originalPath)}";
                        string destPath = Path.Combine(uploadFolder, fileName);
                        File.Copy(originalPath, destPath, true);
                        finalImagePaths.Add($"/uploads/rooms/{fileName}");
                    }
                }
                await _roomService.UploadRoomImagesAsync(roomId, finalImagePaths);

                AppDialog.ShowInfo("Lưu phòng thành công!");
                ToastNotifier.Show(Owner as Form ?? this, "Đã lưu phòng", ToastKind.Success);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (BadRequestException ex)
            {
                AppDialog.ShowWarning(ex.Message, "Lỗi nghiệp vụ");
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