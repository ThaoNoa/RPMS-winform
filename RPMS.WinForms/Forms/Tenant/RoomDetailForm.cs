using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Post;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class RoomDetailForm : Form
    {
        private readonly IPostService _postService;
        private readonly ITenantInteractionService _interactionService;
        private PictureBox picMain = null!;
        private FlowLayoutPanel flpThumbs = null!;
        private Label lblTitle = null!;
        private Label lblPrice = null!;
        private Label lblMeta = null!;
        private Label lblDesc = null!;
        private FlowLayoutPanel flpAmenities = null!;
        private List<string> _images = new();
        private int _index;

        public int PostId { get; set; }
        public PostDto? SeedPost { get; set; }

        public RoomDetailForm(IPostService postService, ITenantInteractionService interactionService)
        {
            _postService = postService;
            _interactionService = interactionService;
            InitializeUI();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyResizableDialog(this, new Size(960, 700));
            Text = "Chi tiết phòng";
            ClientSize = new Size(980, 720);
            BackColor = AppColors.Background;
            StartPosition = FormStartPosition.CenterParent;
            AutoScroll = false;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 10, 16, 10)
            };
            var flpActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var btnBook = new ModernButton { Text = "Đặt lịch xem", Size = new Size(150, 42), BackColor = AppColors.Primary, Margin = new Padding(0, 0, 8, 0) };
            var btnFav = new ModernButton { Text = "Yêu thích", Size = new Size(130, 42), BackColor = AppColors.TextMuted, Margin = new Padding(0, 0, 8, 0) };
            var btnClose = new ModernButton { Text = "Đóng", Size = new Size(110, 42), BackColor = AppColors.Border, ForeColor = AppColors.TextMain };
            btnBook.Click += (s, e) =>
            {
                try
                {
                    var modal = Program.ServiceProvider.GetRequiredService<TenantAppointmentModalForm>();
                    modal.RoomIdToBook = SeedPost?.RoomID ?? 0;
                    modal.RoomInfo = lblTitle.Text;
                    modal.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError("Không mở được lịch hẹn: " + ex.Message);
                }
            };
            btnFav.Click += async (s, e) =>
            {
                try
                {
                    int roomId = SeedPost?.RoomID ?? 0;
                    if (roomId <= 0) return;
                    var isFav = await _interactionService.ToggleFavoriteAsync(UserSession.CurrentUser!.UserID, roomId);
                    AppDialog.ShowInfo(isFav ? "Đã thêm yêu thích" : "Đã bỏ yêu thích");
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError(ex.Message);
                }
            };
            btnClose.Click += (s, e) => Close();
            flpActions.Controls.AddRange(new Control[] { btnBook, btnFav, btnClose });
            pnlBottom.Controls.Add(flpActions);

            var root = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20), BackColor = AppColors.Background };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                BackColor = AppColors.Background
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int row = 0;
            void AddRow(Control c, float height = 0, bool auto = true)
            {
                tbl.RowStyles.Add(auto
                    ? new RowStyle(SizeType.AutoSize)
                    : new RowStyle(SizeType.Absolute, height));
                c.Dock = DockStyle.Fill;
                tbl.Controls.Add(c, 0, row++);
            }

            var pnlGallery = new Panel
            {
                Height = 420,
                BackColor = AppColors.Card,
                Padding = new Padding(16)
            };
            pnlGallery.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlGallery.Width - 1, pnlGallery.Height - 1);
            };

            var tblGallery = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            tblGallery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tblGallery.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56f));
            tblGallery.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tblGallery.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));

            picMain = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(241, 245, 249),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 8)
            };
            picMain.Click += (s, e) =>
            {
                if (_images.Count == 0) return;
                if (ImagePathHelper.IsVideo(_images[_index]))
                    ImagePathHelper.OpenMedia(_images[_index]);
                else
                    ShowFullscreen();
            };

            var pnlNav = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlNav.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            pnlNav.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var btnPrev = new ModernButton { Text = "‹", Dock = DockStyle.Fill, BackColor = AppColors.Primary, Margin = new Padding(0, 0, 0, 4) };
            var btnNext = new ModernButton { Text = "›", Dock = DockStyle.Fill, BackColor = AppColors.Primary, Margin = new Padding(0, 4, 0, 0) };
            btnPrev.Click += (s, e) => ShowImage(_index - 1);
            btnNext.Click += (s, e) => ShowImage(_index + 1);
            pnlNav.Controls.Add(btnPrev, 0, 0);
            pnlNav.Controls.Add(btnNext, 0, 1);

            flpThumbs = new FlowLayoutPanel
            {
                WrapContents = false,
                AutoScroll = true,
                Dock = DockStyle.Fill
            };

            tblGallery.Controls.Add(picMain, 0, 0);
            tblGallery.Controls.Add(pnlNav, 1, 0);
            tblGallery.Controls.Add(flpThumbs, 0, 1);
            tblGallery.SetColumnSpan(flpThumbs, 2);
            pnlGallery.Controls.Add(tblGallery);
            AddRow(pnlGallery, 420, false);

            lblTitle = new Label
            {
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                MaximumSize = new Size(920, 0),
                Padding = new Padding(0, 12, 0, 0)
            };
            AddRow(lblTitle);

            lblPrice = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(920, 0),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                Padding = new Padding(0, 4, 0, 0)
            };
            AddRow(lblPrice);

            lblMeta = new Label
            {
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(920, 0),
                Padding = new Padding(0, 4, 0, 8)
            };
            AddRow(lblMeta);

            AddRow(new Label
            {
                Text = "Tiện nghi",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Padding = new Padding(0, 8, 0, 4)
            });

            flpAmenities = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = true,
                MaximumSize = new Size(920, 0),
                Dock = DockStyle.Top
            };
            AddRow(flpAmenities);

            AddRow(new Label
            {
                Text = "Mô tả",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Padding = new Padding(0, 12, 0, 4)
            });

            lblDesc = new Label
            {
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                MaximumSize = new Size(920, 0),
                Padding = new Padding(0, 0, 0, 12)
            };
            AddRow(lblDesc);

            root.Controls.Add(tbl);
            Controls.Add(root);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                PostDetailDto detail;
                if (PostId > 0)
                {
                    detail = await _postService.GetPostByIdAsync(PostId);
                    await _postService.IncrementViewCountAsync(PostId);
                }
                else if (SeedPost != null)
                {
                    detail = await _postService.GetPostByIdAsync(SeedPost.PostID);
                    PostId = SeedPost.PostID;
                    await _postService.IncrementViewCountAsync(PostId);
                }
                else
                {
                    AppDialog.ShowWarning("Không có thông tin phòng.");
                    Close();
                    return;
                }

                SeedPost = detail;
                Text = detail.Title;
                lblTitle.Text = detail.Title;
                lblPrice.Text = $"{detail.PriceSnapshot:N0} đ/tháng";
                lblMeta.Text = $"Phòng {detail.RoomNumber} · {detail.HouseAddress} · {detail.Area:0.##} m² · Nội thất: {(string.IsNullOrWhiteSpace(detail.Furniture) ? "—" : detail.Furniture)}";
                lblDesc.Text = string.IsNullOrWhiteSpace(detail.Description) ? "Chưa có mô tả chi tiết." : detail.Description;

                flpAmenities.Controls.Clear();
                foreach (var a in detail.Amenities ?? new List<string>())
                {
                    var badge = new Label
                    {
                        Text = "  " + a + "  ",
                        AutoSize = true,
                        Margin = new Padding(0, 0, 8, 4),
                        BackColor = Color.FromArgb(219, 234, 254),
                        ForeColor = AppColors.Primary,
                        Font = new Font("Segoe UI", 9F),
                        Padding = new Padding(6, 4, 6, 4)
                    };
                    flpAmenities.Controls.Add(badge);
                }
                if (flpAmenities.Controls.Count == 0)
                {
                    flpAmenities.Controls.Add(new Label
                    {
                        Text = "Chưa cập nhật tiện nghi",
                        ForeColor = AppColors.TextMuted,
                        AutoSize = true
                    });
                }

                _images = (detail.Images != null && detail.Images.Count > 0)
                    ? detail.Images.ToList()
                    : (string.IsNullOrWhiteSpace(detail.MainImage) ? new List<string>() : new List<string> { detail.MainImage });

                BuildThumbs();
                ShowImage(0);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không tải chi tiết phòng: " + ex.Message);
                Close();
            }
        }

        private void BuildThumbs()
        {
            flpThumbs.Controls.Clear();
            for (int i = 0; i < _images.Count; i++)
            {
                int idx = i;
                var thumb = new PictureBox
                {
                    Size = new Size(56, 36),
                    Margin = new Padding(0, 0, 6, 0),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Cursor = Cursors.Hand,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                ImagePathHelper.ApplyToPictureBox(thumb, _images[i], ImagePathHelper.IsVideo(_images[i]) ? "Video" : "");
                thumb.Click += (s, e) => ShowImage(idx);
                flpThumbs.Controls.Add(thumb);
            }
        }

        private void ShowImage(int index)
        {
            if (_images.Count == 0)
            {
                ImagePathHelper.ApplyToPictureBox(picMain, null, "Chưa có ảnh");
                return;
            }
            if (index < 0) index = _images.Count - 1;
            if (index >= _images.Count) index = 0;
            _index = index;
            ImagePathHelper.ApplyToPictureBox(picMain, _images[_index],
                ImagePathHelper.IsVideo(_images[_index]) ? "▶ Video — nhấn để mở" : "Không tải được ảnh");
        }

        private void ShowFullscreen()
        {
            if (_images.Count == 0) return;
            if (ImagePathHelper.IsVideo(_images[_index]))
            {
                ImagePathHelper.OpenMedia(_images[_index]);
                return;
            }
            using var f = new Form
            {
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Black,
                FormBorderStyle = FormBorderStyle.None,
                KeyPreview = true
            };
            var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            ImagePathHelper.ApplyToPictureBox(pic, _images[_index]);
            f.Controls.Add(pic);
            f.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) f.Close(); };
            pic.Click += (s, e) => f.Close();
            f.ShowDialog(this);
        }
    }
}

