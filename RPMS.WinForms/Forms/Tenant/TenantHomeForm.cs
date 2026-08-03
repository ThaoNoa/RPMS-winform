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
        private Label lblResult = null!;
        private Label lblEmpty = null!;
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
            UIHelper.ApplyFormStyle(this);
            MinimumSize = new Size(900, 560);
            ClientSize = new Size(1180, 740);
            BackColor = AppColors.Background;
            DoubleBuffered = true;
            AutoScroll = false;

            pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 210,
                MinimumSize = new Size(0, 200),
                AutoScroll = true,
                BackColor = AppColors.Card,
                Padding = new Padding(16)
            };
            pnlFilter.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            };

            var lblHead = new Label
            {
                Text = "Tìm phòng cho thuê",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 12),
                AutoSize = true
            };

            // Hàng 1 — labels riêng
            AddFieldLabel(pnlFilter, "Từ khóa", 20, 48);
            txtSearch = MkText(20, 70, 220, "Tên phòng, địa chỉ…");

            AddFieldLabel(pnlFilter, "Giá từ", 256, 48);
            txtMinPrice = MkText(256, 70, 110, "VD: 2000000");

            AddFieldLabel(pnlFilter, "Giá đến", 380, 48);
            txtMaxPrice = MkText(380, 70, 110, "VD: 5000000");

            AddFieldLabel(pnlFilter, "Thành phố", 506, 48);
            txtCity = MkText(506, 70, 140, "TP.HCM…");

            AddFieldLabel(pnlFilter, "Quận / Huyện", 662, 48);
            txtDistrict = MkText(662, 70, 140, "Quận 1…");

            AddFieldLabel(pnlFilter, "Diện tích", 818, 48);
            cboArea = new ComboBox
            {
                Location = new Point(818, 72),
                Size = new Size(140, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            cboArea.Items.AddRange(new object[] { "Tất cả", "Dưới 25m²", "25–50m²", "50–100m²", ">100m²" });
            cboArea.SelectedIndex = 0;

            AddFieldLabel(pnlFilter, "Phòng ngủ", 974, 48);
            cboBedrooms = new ComboBox
            {
                Location = new Point(974, 72),
                Size = new Size(90, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            cboBedrooms.Items.AddRange(new object[] { "Tất cả", "1", "2", "3", "4+" });
            cboBedrooms.SelectedIndex = 0;

            // Hàng 2 — tiện nghi + sort
            chkAc = MkCheck("Điều hòa", 20, 118);
            chkWifi = MkCheck("Wifi", 120, 118);
            chkWasher = MkCheck("Máy giặt", 190, 118);
            chkFurniture = MkCheck("Nội thất", 300, 118);
            chkPet = MkCheck("Thú cưng", 400, 118);
            chkParking = MkCheck("Chỗ để xe", 510, 118);

            AddFieldLabel(pnlFilter, "Đánh giá", 640, 100);
            cboRating = new ComboBox
            {
                Location = new Point(640, 120),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            cboRating.Items.AddRange(new object[] { "Mọi rating", "Từ 3★", "Từ 4★", "5★" });
            cboRating.SelectedIndex = 0;

            AddFieldLabel(pnlFilter, "Sắp xếp", 780, 100);
            cboSort = new ComboBox
            {
                Location = new Point(780, 120),
                Size = new Size(130, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            cboSort.Items.AddRange(new object[] { "Mới nhất", "Giá tăng", "Giá giảm", "Nổi bật" });
            cboSort.SelectedIndex = 0;

            var btnSearch = new ModernButton
            {
                Text = "Tìm kiếm",
                Location = new Point(930, 114),
                Size = new Size(120, 40),
                BackColor = AppColors.Primary
            };
            btnSearch.Click += async (s, e) => await PerformSearchAsync();

            var btnClear = new ModernButton
            {
                Text = "Xóa lọc",
                Location = new Point(1060, 114),
                Size = new Size(100, 40),
                BackColor = AppColors.Border,
                ForeColor = AppColors.TextMain
            };
            btnClear.Click += async (s, e) =>
            {
                txtSearch.Text = "";
                txtMinPrice.Text = "";
                txtMaxPrice.Text = "";
                txtCity.Text = "";
                txtDistrict.Text = "";
                cboArea.SelectedIndex = 0;
                cboBedrooms.SelectedIndex = 0;
                cboSort.SelectedIndex = 0;
                cboRating.SelectedIndex = 0;
                chkAc.Checked = chkWifi.Checked = chkWasher.Checked = false;
                chkFurniture.Checked = chkPet.Checked = chkParking.Checked = false;
                await PerformSearchAsync();
            };

            lblResult = new Label
            {
                Location = new Point(20, 168),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                Text = "Đang tải…"
            };

            pnlFilter.Controls.AddRange(new Control[]
            {
                lblHead, txtSearch, txtMinPrice, txtMaxPrice, txtCity, txtDistrict,
                cboArea, cboBedrooms, chkAc, chkWifi, chkWasher, chkFurniture, chkPet, chkParking,
                cboRating, cboSort, btnSearch, btnClear, lblResult
            });

            flpRooms = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(16),
                BackColor = AppColors.Background
            };

            lblEmpty = new Label
            {
                Text = "Không tìm thấy phòng phù hợp.\nThử đổi bộ lọc hoặc xóa lọc.",
                Font = new Font("Segoe UI", 12F),
                ForeColor = AppColors.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            Controls.Add(flpRooms);
            Controls.Add(lblEmpty);
            Controls.Add(pnlFilter);

            AcceptButton = null;
            txtSearch.InputKeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await PerformSearchAsync();
                }
            };
        }

        private static void AddFieldLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextMuted
            });
        }

        private static ModernTextBox MkText(int x, int y, int w, string placeholder) => new()
        {
            Location = new Point(x, y),
            Size = new Size(w, 34),
            PlaceholderText = placeholder
        };

        private static CheckBox MkCheck(string text, int x, int y) => new()
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            Font = AppTypography.Body,
            ForeColor = AppColors.TextMain
        };

        private async System.Threading.Tasks.Task PerformSearchAsync()
        {
            try
            {
                lblResult.Text = "Đang tìm…";
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
                if (IsDisposed || !IsHandleCreated) return;

                flpRooms.SuspendLayout();
                flpRooms.Controls.Clear();
                int count = 0;
                foreach (var post in posts)
                {
                    count++;
                    var card = new RoomCardControl(post);
                    card.OnCardClicked += (s, p) => OpenDetail(p);
                    card.OnBookClicked += (s, p) =>
                    {
                        if (IsDisposed) return;
                        try
                        {
                            var modal = Program.ServiceProvider.GetRequiredService<TenantAppointmentModalForm>();
                            modal.RoomIdToBook = p.RoomID;
                            modal.RoomInfo = $"Phòng {p.RoomNumber} - {p.HouseAddress}";
                            modal.ShowDialog(this);
                        }
                        catch (Exception ex)
                        {
                            AppDialog.ShowError("Không mở được lịch hẹn: " + ex.Message);
                        }
                    };
                    card.OnFavoriteClicked += async (s, p) =>
                    {
                        try
                        {
                            var isFav = await _interactionService.ToggleFavoriteAsync(UserSession.CurrentUser!.UserID, p.RoomID);
                            if (!IsDisposed)
                                AppDialog.ShowInfo(isFav ? "Đã thêm vào yêu thích" : "Đã xóa khỏi yêu thích");
                        }
                        catch (Exception ex)
                        {
                            if (!IsDisposed)
                                AppDialog.ShowError("Lỗi yêu thích: " + ex.Message);
                        }
                    };
                    flpRooms.Controls.Add(card);
                }
                flpRooms.ResumeLayout();

                lblEmpty.Visible = count == 0;
                flpRooms.Visible = count > 0;
                lblResult.Text = count == 0 ? "Không có kết quả" : $"Tìm thấy {count} phòng";
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tìm được phòng: " + ex.Message);
            }
        }

        private void OpenDetail(PostDto post)
        {
            try
            {
                var detail = Program.ServiceProvider.GetRequiredService<RoomDetailForm>();
                detail.PostId = post.PostID;
                detail.SeedPost = post;
                detail.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không mở chi tiết phòng: " + ex.Message);
            }
        }
    }
}
