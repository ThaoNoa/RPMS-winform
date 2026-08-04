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
        private ModernTextBox txtSearch = null!, txtMinPrice = null!, txtMaxPrice = null!, txtCity = null!, txtDistrict = null!;
        private ComboBox cboArea = null!, cboBedrooms = null!, cboSort = null!, cboRating = null!, cboStatus = null!;
        private CheckBox chkAc = null!, chkWifi = null!, chkWasher = null!, chkFurniture = null!, chkPet = null!, chkParking = null!, chkFeatured = null!;
        private LoadingPanel _loading = null!;
        private EmptyStatePanel _empty = null!;

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

            var header = UIHelper.CreatePageHeader("Tìm phòng cho thuê");

            pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoScroll = true,
                BackColor = AppColors.Card,
                Padding = new Padding(AppLayout.PagePadding, 8, AppLayout.PagePadding, 10),
                MinimumSize = new Size(0, 120)
            };
            pnlFilter.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            };

            var flpFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = AppColors.Card,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            txtSearch = MkText(220, "Tên phòng, địa chỉ…");
            txtMinPrice = MkText(110, "VD: 2000000");
            txtMaxPrice = MkText(110, "VD: 5000000");
            txtCity = MkText(140, "TP.HCM…");
            txtDistrict = MkText(140, "Quận 1…");

            cboArea = new ComboBox();
            UIHelper.StyleCombo(cboArea);
            cboArea.Items.AddRange(new object[] { "Tất cả", "Dưới 25m²", "25–50m²", "50–100m²", ">100m²" });
            cboArea.SelectedIndex = 0;

            cboBedrooms = new ComboBox();
            UIHelper.StyleCombo(cboBedrooms);
            cboBedrooms.Items.AddRange(new object[] { "Tất cả", "1", "2", "3", "4+" });
            cboBedrooms.SelectedIndex = 0;

            cboRating = new ComboBox();
            UIHelper.StyleCombo(cboRating);
            cboRating.Items.AddRange(new object[] { "Mọi rating", "Từ 3★", "Từ 4★", "5★" });
            cboRating.SelectedIndex = 0;

            cboSort = new ComboBox();
            UIHelper.StyleCombo(cboSort);
            cboSort.Items.AddRange(new object[] { "Mới nhất", "Giá tăng", "Giá giảm", "Nổi bật" });
            cboSort.SelectedIndex = 0;

            cboStatus = new ComboBox();
            UIHelper.StyleCombo(cboStatus);
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Còn trống", "Đã thuê" });
            cboStatus.SelectedIndex = 1;

            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Từ khóa", txtSearch, 220));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Giá từ", txtMinPrice, 110));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Giá đến", txtMaxPrice, 110));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Thành phố", txtCity, 140));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Quận / Huyện", txtDistrict, 140));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Diện tích", cboArea, 140));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Phòng ngủ", cboBedrooms, 90));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Đánh giá", cboRating, 120));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Sắp xếp", cboSort, 130));
            flpFilters.Controls.Add(UIHelper.CreateLabeledField("Trạng thái", cboStatus, 120));

            var flpAmenities = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 18, AppLayout.FieldGap, 6),
                Padding = new Padding(0, 4, 0, 0)
            };
            chkAc = MkCheck("Điều hòa");
            chkWifi = MkCheck("Wifi");
            chkWasher = MkCheck("Máy giặt");
            chkFurniture = MkCheck("Nội thất");
            chkPet = MkCheck("Thú cưng");
            chkParking = MkCheck("Chỗ để xe");
            chkFeatured = MkCheck("Chỉ tin nổi bật");
            flpAmenities.Controls.AddRange(new Control[]
            {
                chkAc, chkWifi, chkWasher, chkFurniture, chkPet, chkParking, chkFeatured
            });
            flpFilters.Controls.Add(flpAmenities);

            var btnSearch = UIHelper.PrimaryButton("Tìm kiếm", 120);
            btnSearch.Click += async (s, e) => await PerformSearchAsync();

            var btnClear = UIHelper.SecondaryButton("Xóa lọc", 100);
            btnClear.BackColor = AppColors.Border;
            btnClear.ForeColor = AppColors.TextMain;
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
                cboStatus.SelectedIndex = 1;
                chkFeatured.Checked = false;
                chkAc.Checked = chkWifi.Checked = chkWasher.Checked = false;
                chkFurniture.Checked = chkPet.Checked = chkParking.Checked = false;
                await PerformSearchAsync();
            };

            lblResult = new Label
            {
                AutoSize = true,
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                Text = "Đang tải…",
                Margin = new Padding(12, 12, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var flpActions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 12, 0, 0),
                Padding = new Padding(0)
            };
            btnSearch.Margin = new Padding(0, 0, AppLayout.FieldGap, 0);
            btnClear.Margin = new Padding(0, 0, AppLayout.FieldGap, 0);
            flpActions.Controls.Add(btnSearch);
            flpActions.Controls.Add(btnClear);
            flpActions.Controls.Add(lblResult);
            flpFilters.Controls.Add(flpActions);

            pnlFilter.Controls.Add(flpFilters);

            flpRooms = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(AppLayout.PagePadding),
                BackColor = AppColors.Background
            };

            _empty = new EmptyStatePanel();
            _loading = new LoadingPanel();

            Controls.Add(_loading);
            Controls.Add(flpRooms);
            Controls.Add(_empty);
            Controls.Add(pnlFilter);
            Controls.Add(header);

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

        private static ModernTextBox MkText(int w, string placeholder) => new()
        {
            Size = new Size(w, AppLayout.InputHeight),
            PlaceholderText = placeholder
        };

        private static CheckBox MkCheck(string text) => new()
        {
            Text = text,
            AutoSize = true,
            Font = AppTypography.Body,
            ForeColor = AppColors.TextMain,
            Margin = new Padding(0, 4, 12, 4)
        };

        private async System.Threading.Tasks.Task PerformSearchAsync()
        {
            try
            {
                lblResult.Text = "Đang tìm…";
                _loading.ShowLoading("Đang tìm phòng…");
                _empty.HideEmpty();
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
                    RoomStatus = cboStatus.SelectedIndex switch
                    {
                        1 => "Available",
                        2 => "Occupied",
                        _ => null
                    },
                    FeaturedOnly = chkFeatured.Checked ? true : null,
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
                                ToastNotifier.Show(this, isFav ? "Đã thêm yêu thích" : "Đã bỏ yêu thích", ToastKind.Success);
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

                if (count == 0)
                {
                    flpRooms.Visible = false;
                    _empty.ShowEmpty("Không tìm thấy phòng phù hợp", "Thử đổi giá, diện tích, tiện ích hoặc xóa bộ lọc.");
                    lblResult.Text = "Không có kết quả";
                }
                else
                {
                    _empty.HideEmpty();
                    flpRooms.Visible = true;
                    lblResult.Text = $"Tìm thấy {count} phòng";
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tìm được phòng: " + ex.Message);
            }
            finally
            {
                _loading.HideLoading();
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
