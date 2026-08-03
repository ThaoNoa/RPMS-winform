using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Post;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantHomeForm : Form
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantInteractionService _interactionService;
        private Panel pnlFilter = null!;
        private FlowLayoutPanel flpRooms = null!;
        private ModernTextBox txtSearch = null!, txtMinPrice = null!, txtMaxPrice = null!, txtCity = null!, txtDistrict = null!;
        private ComboBox cboArea = null!, cboBedrooms = null!, cboSort = null!, cboRating = null!;
        private CheckBox chkAc = null!, chkWifi = null!, chkWasher = null!, chkFurniture = null!, chkPet = null!, chkParking = null!;

        public TenantHomeForm(ITenantService tenantService, ITenantInteractionService interactionService)
        {
            _tenantService = tenantService;
            _interactionService = interactionService;
            InitializeUI();
            Load += async (s, e) => await PerformSearchAsync();
        }

        private void InitializeUI()
        {
            ClientSize = new Size(1180, 740);
            BackColor = AppColors.Background;

            pnlFilter = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = AppColors.Card, Padding = new Padding(12) };
            txtSearch = new ModernTextBox { Location = new Point(20, 30), Size = new Size(200, 32) };
            txtMinPrice = new ModernTextBox { Location = new Point(240, 30), Size = new Size(90, 32) };
            txtMaxPrice = new ModernTextBox { Location = new Point(340, 30), Size = new Size(90, 32) };
            txtCity = new ModernTextBox { Location = new Point(450, 30), Size = new Size(120, 32) };
            txtDistrict = new ModernTextBox { Location = new Point(580, 30), Size = new Size(120, 32) };

            cboArea = new ComboBox { Location = new Point(720, 32), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboArea.Items.AddRange(new object[] { "Tất cả DT", "Dưới 25m2", "25-50m2", "50-100m2", ">100m2" });
            cboArea.SelectedIndex = 0;
            cboBedrooms = new ComboBox { Location = new Point(860, 32), Size = new Size(90, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBedrooms.Items.AddRange(new object[] { "PN", "1", "2", "3", "4+" });
            cboBedrooms.SelectedIndex = 0;
            cboSort = new ComboBox { Location = new Point(960, 32), Size = new Size(120, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboSort.Items.AddRange(new object[] { "Mới nhất", "Giá tăng", "Giá giảm", "Nổi bật" });
            cboSort.SelectedIndex = 0;

            chkAc = new CheckBox { Text = "Điều hòa", Location = new Point(20, 85), AutoSize = true };
            chkWifi = new CheckBox { Text = "Wifi", Location = new Point(120, 85), AutoSize = true };
            chkWasher = new CheckBox { Text = "Máy giặt", Location = new Point(190, 85), AutoSize = true };
            chkFurniture = new CheckBox { Text = "Nội thất", Location = new Point(290, 85), AutoSize = true };
            chkPet = new CheckBox { Text = "Thú cưng", Location = new Point(390, 85), AutoSize = true };
            chkParking = new CheckBox { Text = "Chỗ để xe", Location = new Point(500, 85), AutoSize = true };
            cboRating = new ComboBox { Location = new Point(620, 83), Size = new Size(120, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRating.Items.AddRange(new object[] { "Mọi rating", "Từ 3★", "Từ 4★", "5★" });
            cboRating.SelectedIndex = 0;

            var btnSearch = new ModernButton { Text = "Lọc", Location = new Point(780, 78), Size = new Size(100, 36) };
            btnSearch.Click += async (s, e) => await PerformSearchAsync();

            pnlFilter.Controls.AddRange(new Control[]
            {
                new Label { Text = "Từ khóa / Giá / Thành phố / Quận", Location = new Point(20, 8), AutoSize = true, ForeColor = AppColors.TextMuted },
                txtSearch, txtMinPrice, txtMaxPrice, txtCity, txtDistrict, cboArea, cboBedrooms, cboSort,
                chkAc, chkWifi, chkWasher, chkFurniture, chkPet, chkParking, cboRating, btnSearch
            });

            flpRooms = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(15), BackColor = AppColors.Background };
            Controls.Add(flpRooms);
            Controls.Add(pnlFilter);
        }

        private async System.Threading.Tasks.Task PerformSearchAsync()
        {
            var filter = new RoomSearchFilterDto
            {
                Keyword = txtSearch.Text,
                MinPrice = decimal.TryParse(txtMinPrice.Text, out var m1) ? m1 : null,
                MaxPrice = decimal.TryParse(txtMaxPrice.Text, out var m2) ? m2 : null,
                City = txtCity.Text,
                District = txtDistrict.Text,
                AreaFilter = cboArea.SelectedIndex == 0 ? null : cboArea.SelectedIndex,
                Bedrooms = cboBedrooms.SelectedIndex == 0 ? null : cboBedrooms.SelectedIndex,
                HasAirConditioner = chkAc.Checked ? true : null,
                HasWifi = chkWifi.Checked ? true : null,
                HasWashingMachine = chkWasher.Checked ? true : null,
                HasFurniture = chkFurniture.Checked ? true : null,
                AllowPet = chkPet.Checked ? true : null,
                HasParking = chkParking.Checked ? true : null,
                MinRating = cboRating.SelectedIndex switch { 1 => 3, 2 => 4, 3 => 5, _ => null },
                SortBy = cboSort.SelectedIndex switch
                {
                    1 => "PriceAsc",
                    2 => "PriceDesc",
                    3 => "Rating",
                    _ => "Newest"
                }
            };

            var posts = await _tenantService.SearchRoomsAsync(filter);
            flpRooms.Controls.Clear();
            foreach (var post in posts)
            {
                var card = new RoomCardControl(post);
                card.OnBookClicked += (s, p) =>
                {
                    var modal = Program.ServiceProvider.GetRequiredService<TenantAppointmentModalForm>();
                    modal.RoomIdToBook = p.RoomID;
                    modal.RoomInfo = $"Phòng {p.RoomNumber} - {p.HouseAddress}";
                    modal.ShowDialog();
                };
                card.OnFavoriteClicked += async (s, p) =>
                {
                    var isFav = await _interactionService.ToggleFavoriteAsync(UserSession.CurrentUser!.UserID, p.RoomID);
                    AppDialog.ShowInfo(isFav ? "Đã thêm vào yêu thích" : "Đã xóa khỏi yêu thích");
                };
                flpRooms.Controls.Add(card);
            }
        }
    }
}
