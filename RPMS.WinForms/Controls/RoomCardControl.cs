using RPMS.Common.Constants;
using RPMS.DTO.Post;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class RoomCardControl : UserControl
    {
        private PictureBox picMain = null!;
        private Label lblTitle = null!;
        private Label lblPrice = null!;
        private Label lblAddress = null!;
        private ModernButton btnBook = null!;
        private ModernButton btnFavorite = null!;
        public PostDto PostData { get; private set; } = null!;
        public event EventHandler<PostDto>? OnBookClicked;
        public event EventHandler<PostDto>? OnFavoriteClicked;

        public RoomCardControl(PostDto post)
        {
            PostData = post;
            InitializeCard();
            BindData();
        }

        private void InitializeCard()
        {
            this.Size = new Size(280, 360);
            this.BackColor = AppColors.Card;
            this.Margin = new Padding(15);
            this.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppColors.Border, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            picMain = new PictureBox
            {
                Location = new Point(1, 1),
                Size = new Size(278, 180),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = AppColors.Background
            };
            lblTitle = new Label
            {
                Location = new Point(10, 190),
                Size = new Size(260, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoEllipsis = true
            };
            lblPrice = new Label
            {
                Location = new Point(10, 240),
                Size = new Size(260, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.Danger
            };
            lblAddress = new Label
            {
                Location = new Point(10, 265),
                Size = new Size(260, 40),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                AutoEllipsis = true
            };
            btnBook = new ModernButton
            {
                Location = new Point(10, 310),
                Size = new Size(130, 35),
                Text = "Đặt lịch xem",
                BackColor = AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnBook.Click += (s, e) => OnBookClicked?.Invoke(this, PostData);

            btnFavorite = new ModernButton
            {
                Location = new Point(150, 310),
                Size = new Size(120, 35),
                Text = "Yêu thích",
                BackColor = AppColors.Secondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnFavorite.Click += (s, e) => OnFavoriteClicked?.Invoke(this, PostData);

            this.Controls.AddRange(new Control[] { picMain, lblTitle, lblPrice, lblAddress, btnBook, btnFavorite });
        }

        private void BindData()
        {
            lblTitle.Text = PostData.Title;
            lblPrice.Text = $"{PostData.PriceSnapshot:N0} VNĐ/Tháng";
            lblAddress.Text = $"Phòng {PostData.RoomNumber} - {PostData.HouseAddress}";

            if (!string.IsNullOrEmpty(PostData.MainImage))
            {
                string path = PostData.MainImage;
                if (path.StartsWith("/"))
                    path = Path.Combine(Application.StartupPath, path.TrimStart('/'));
                if (File.Exists(path))
                {
                    picMain.SizeMode = PictureBoxSizeMode.Zoom;
                    picMain.ImageLocation = path;
                }
            }
        }
    }
}