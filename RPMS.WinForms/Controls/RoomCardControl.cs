using RPMS.Common.Constants;
using RPMS.DTO.Post;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class RoomCardControl : UserControl
    {
        private PictureBox picMain = null!;
        private Label lblTitle = null!;
        private Label lblPrice = null!;
        private Label lblAddress = null!;
        private Label lblHint = null!;
        private ModernButton btnBook = null!;
        private ModernButton btnFavorite = null!;
        public PostDto PostData { get; private set; } = null!;
        public event EventHandler<PostDto>? OnBookClicked;
        public event EventHandler<PostDto>? OnFavoriteClicked;
        public event EventHandler<PostDto>? OnCardClicked;

        public RoomCardControl(PostDto post)
        {
            PostData = post;
            InitializeCard();
            BindData();
        }

        private void InitializeCard()
        {
            Size = new Size(300, 390);
            BackColor = AppColors.Card;
            Margin = new Padding(12);
            Cursor = Cursors.Hand;
            Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 12);
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawPath(pen, path);
            };

            picMain = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(276, 170),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppColors.Background,
                Cursor = Cursors.Hand
            };
            picMain.Click += (s, e) => OnCardClicked?.Invoke(this, PostData);

            lblTitle = new Label
            {
                Location = new Point(14, 192),
                Size = new Size(270, 42),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblTitle.Click += (s, e) => OnCardClicked?.Invoke(this, PostData);

            lblPrice = new Label
            {
                Location = new Point(14, 238),
                Size = new Size(270, 24),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.Primary
            };

            lblAddress = new Label
            {
                Location = new Point(14, 264),
                Size = new Size(270, 36),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                AutoEllipsis = true
            };

            lblHint = new Label
            {
                Location = new Point(14, 302),
                Size = new Size(270, 18),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Italic),
                ForeColor = AppColors.TextMuted,
                Text = "Nhấn ảnh / tiêu đề để xem chi tiết"
            };

            btnBook = new ModernButton
            {
                Location = new Point(14, 330),
                Size = new Size(130, 36),
                Text = "Đặt lịch",
                BackColor = AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnBook.Click += (s, e) => OnBookClicked?.Invoke(this, PostData);

            btnFavorite = new ModernButton
            {
                Location = new Point(154, 330),
                Size = new Size(130, 36),
                Text = "Yêu thích",
                BackColor = AppColors.TextMuted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnFavorite.Click += (s, e) => OnFavoriteClicked?.Invoke(this, PostData);

            Controls.AddRange(new Control[] { picMain, lblTitle, lblPrice, lblAddress, lblHint, btnBook, btnFavorite });
        }

        private void BindData()
        {
            lblTitle.Text = string.IsNullOrWhiteSpace(PostData.Title) ? $"Phòng {PostData.RoomNumber}" : PostData.Title;
            lblPrice.Text = $"{PostData.PriceSnapshot:N0} đ/tháng";
            lblAddress.Text = $"Phòng {PostData.RoomNumber} · {PostData.HouseAddress}";
            ImagePathHelper.ApplyToPictureBox(picMain, PostData.MainImage, "Chưa có ảnh phòng");
        }
    }
}
