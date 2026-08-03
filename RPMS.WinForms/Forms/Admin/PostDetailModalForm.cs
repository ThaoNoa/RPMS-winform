using RPMS.BLL.Interfaces;
using RPMS.WinForms.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    public partial class PostDetailModalForm : Form
    {
        private readonly IPostService _postService;
        public int PostId { get; set; }

        public PostDetailModalForm(IPostService postService)
        {
            InitializeComponent();
            _postService = postService;
            this.Load += PostDetailModalForm_Load!;
        }

        private async void PostDetailModalForm_Load(object sender, EventArgs e)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(PostId);
                string content = $"TIÊU ĐỀ: {post.Title}\n" +
                    $"====================================\n\n" +
                    $"Giá thuê: {post.PriceSnapshot:N0} VNĐ\n" +
                    $"Phòng: {post.RoomNumber} (Diện tích: {post.Area}m2)\n" +
                    $"Địa chỉ: {post.HouseAddress}\n" +
                    $"Trạng thái: {post.Status}\n" +
                    $"Lượt xem: {post.ViewCount}\n" +
                    $"Ngày hết hạn: {(post.ExpiryDate?.ToString("dd/MM/yyyy") ?? "N/A")}\n\n" +
                    $"MÔ TẢ:\n{post.Description}\n\n" +
                    $"NỘI THẤT:\n{post.Furniture}\n\n" +
                    $"TIỆN ÍCH: {string.Join(", ", post.Amenities)}\n";
                rtxtContent.Text = content;
                UIHelper.SoftAnchorDialogControls(this);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không thể tải chi tiết tin đăng: " + ex.Message);
                this.Close();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}