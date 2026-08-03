using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.House;
using RPMS.DTO.Post;
using RPMS.DTO.Room;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordPostForm : Form
    {
        private readonly IPostService _postService;
        private readonly IHouseService _houseService;
        private readonly IRoomService _roomService;

        private ComboBox cboHouse = null!;
        private ComboBox cboRoom = null!;
        private ModernTextBox txtTitle = null!;
        private ModernTextBox txtPrice = null!;
        private TextBox txtDesc = null!;
        private ModernButton btnPost = null!;
        private List<HouseDto> _houses = new();
        private List<RoomDto> _rooms = new();

        public int RoomIdToPost { get; set; }

        public LandlordPostForm(IPostService postService, IHouseService houseService, IRoomService roomService)
        {
            _postService = postService;
            _houseService = houseService;
            _roomService = roomService;
            InitializeUI();
            Load += LandlordPostForm_Load!;
        }

        private void InitializeUI()
        {
            ClientSize = new Size(640, 520);
            BackColor = AppColors.Card;
            Text = "Tạo Tin Đăng Cho Thuê";
            StartPosition = FormStartPosition.CenterParent;

            var lblHouse = new Label { Text = "Chọn nhà *", Location = new Point(30, 25), AutoSize = true };
            cboHouse = new ComboBox
            {
                Location = new Point(30, 50),
                Size = new Size(560, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboHouse.SelectedIndexChanged += async (s, e) => await LoadRoomsForSelectedHouseAsync();

            var lblRoom = new Label { Text = "Chọn phòng trống *", Location = new Point(30, 95), AutoSize = true };
            cboRoom = new ComboBox
            {
                Location = new Point(30, 120),
                Size = new Size(560, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRoom.SelectedIndexChanged += CboRoom_SelectedIndexChanged;

            var lblTitle = new Label { Text = "Tiêu đề tin đăng *", Location = new Point(30, 165), AutoSize = true };
            txtTitle = new ModernTextBox { Location = new Point(30, 190), Size = new Size(560, 35) };

            var lblPrice = new Label { Text = "Giá đăng (VNĐ) *", Location = new Point(30, 240), AutoSize = true };
            txtPrice = new ModernTextBox { Location = new Point(30, 265), Size = new Size(250, 35) };

            var lblDesc = new Label { Text = "Nội dung quảng cáo", Location = new Point(30, 315), AutoSize = true };
            txtDesc = new TextBox
            {
                Location = new Point(30, 340),
                Size = new Size(560, 90),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnPost = new ModernButton
            {
                Text = "Gửi Duyệt",
                Location = new Point(230, 450),
                Size = new Size(160, 40),
                BackColor = AppColors.Primary
            };
            btnPost.Click += BtnPost_Click!;

            Controls.AddRange(new Control[]
            {
                lblHouse, cboHouse, lblRoom, cboRoom, lblTitle, txtTitle, lblPrice, txtPrice, lblDesc, txtDesc, btnPost
            });
        }

        private async void LandlordPostForm_Load(object sender, EventArgs e)
        {
            try
            {
                var landlordId = UserSession.CurrentUser!.UserID;
                _houses = (await _houseService.GetHousesByOwnerAsync(landlordId)).ToList();
                cboHouse.DataSource = _houses;
                cboHouse.DisplayMember = "HouseName";
                cboHouse.ValueMember = "HouseID";

                if (_houses.Count == 0)
                {
                    btnPost.Enabled = false;
                    AppDialog.ShowInfo("Bạn chưa có nhà nào. Vui lòng thêm nhà và phòng trước.");
                    return;
                }

                await LoadRoomsForSelectedHouseAsync();

                if (RoomIdToPost > 0)
                    SelectRoomById(RoomIdToPost);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async Task LoadRoomsForSelectedHouseAsync()
        {
            cboRoom.DataSource = null;
            if (cboHouse.SelectedValue == null || !int.TryParse(cboHouse.SelectedValue.ToString(), out int houseId))
                return;

            var rooms = await _roomService.GetRoomsByHouseAsync(houseId);
            _rooms = rooms.Where(r => r.Status == "Available").ToList();
            cboRoom.DataSource = _rooms;
            cboRoom.DisplayMember = "RoomNumber";
            cboRoom.ValueMember = "RoomID";
            btnPost.Enabled = _rooms.Count > 0;
        }

        private void SelectRoomById(int roomId)
        {
            foreach (var house in _houses)
            {
                // Will reload rooms when house changes; try match after load
            }

            var room = _rooms.FirstOrDefault(r => r.RoomID == roomId);
            if (room != null)
                cboRoom.SelectedValue = roomId;
        }

        private void CboRoom_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboRoom.SelectedItem is RoomDto room)
            {
                RoomIdToPost = room.RoomID;
                if (string.IsNullOrWhiteSpace(txtPrice.Text) || txtPrice.Text == "0")
                    txtPrice.Text = room.Price.ToString("0");
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                    txtTitle.Text = $"Cho thuê phòng {room.RoomNumber}";
            }
        }

        private async void BtnPost_Click(object sender, EventArgs e)
        {
            if (cboRoom.SelectedValue == null || !int.TryParse(cboRoom.SelectedValue.ToString(), out int roomId))
            {
                AppDialog.ShowWarning("Vui lòng chọn phòng trống để đăng tin.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                AppDialog.ShowWarning("Vui lòng nhập tiêu đề và giá hợp lệ.");
                return;
            }

            try
            {
                btnPost.Enabled = false;
                await _postService.CreatePostAsync(new CreatePostDto
                {
                    RoomID = roomId,
                    Title = txtTitle.Text.Trim(),
                    Description = txtDesc.Text.Trim(),
                    PriceSnapshot = price,
                    ExpiryMonths = 1
                });
                AppDialog.ShowInfo("Đã gửi tin đăng! Vui lòng chờ Admin duyệt.", "Thành công");
                txtTitle.Text = "";
                txtDesc.Text = "";
                txtPrice.Text = "";
                await LoadRoomsForSelectedHouseAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                btnPost.Enabled = _rooms.Count > 0;
            }
        }
    }
}
