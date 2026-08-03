using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DTO.Post;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace RPMS.WinForms.Forms.Admin
{
    public partial class PostManagementForm : Form
    {
        private readonly IPostService _postService;
        private List<PostDto> _posts;

        public PostManagementForm(IPostService postService)
        {
            InitializeComponent();
            _postService = postService;
            _posts = new List<PostDto>();
            SetupDataGridView();
            this.Load += PostManagementForm_Load!;
        }

        private void SetupDataGridView()
        {
            dgvPosts.AutoGenerateColumns = false;
            dgvPosts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PostID", HeaderText = "ID", Width = 50 });
            dgvPosts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", Width = 200 });
            dgvPosts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 80 });
            dgvPosts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PriceSnapshot", HeaderText = "Giá thuê", Width = 100 });
            dgvPosts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgvPosts.Columns.Add(new DataGridViewLinkColumn { HeaderText = "Chi tiết", Text = "Xem", UseColumnTextForLinkValue = true, Name = "ViewCol", Width = 60 });
            dgvPosts.Columns.Add(new DataGridViewLinkColumn { HeaderText = "Duyệt", Text = "Duyệt", UseColumnTextForLinkValue = true, Name = "ApproveCol", Width = 60 });
            dgvPosts.Columns.Add(new DataGridViewLinkColumn { HeaderText = "Từ chối", Text = "Từ chối", UseColumnTextForLinkValue = true, Name = "RejectCol", Width = 60 });
        }

        private async void PostManagementForm_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void cboStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (cboStatusFilter.SelectedIndex == 0)
                {
                    var pendingPosts = await _postService.GetPendingPostsAsync();
                    _posts = pendingPosts.ToList();
                }
                else
                {
                    var activePosts = await _postService.GetAllActivePostsAsync();
                    _posts = activePosts.ToList();
                }
                BindData(_posts);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void BindData(List<PostDto> data)
        {
            dgvPosts.DataSource = null;
            dgvPosts.DataSource = data;
            bool isPending = cboStatusFilter.SelectedIndex == 0;
            dgvPosts.Columns["ApproveCol"].Visible = isPending;
            dgvPosts.Columns["RejectCol"].Visible = isPending;
        }

        private async void dgvPosts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var post = dgvPosts.Rows[e.RowIndex].DataBoundItem as PostDto;
            if (post == null) return;

            string colName = dgvPosts.Columns[e.ColumnIndex].Name;
            try
            {
                if (colName == "ViewCol")
                {
                    var modal = Program.ServiceProvider.GetRequiredService<PostDetailModalForm>();
                    modal.PostId = post.PostID;
                    modal.ShowDialog();
                }
                else if (colName == "ApproveCol")
                {
                    if (AppDialog.Confirm($"Duyệt tin đăng '{post.Title}'?"))
                    {
                        await _postService.ApprovePostAsync(post.PostID, UserSession.CurrentUser!.UserID);
                        AppDialog.ShowInfo("Đã duyệt tin thành công.");
                        await LoadDataAsync();
                    }
                }
                else if (colName == "RejectCol")
                {
                    if (AppDialog.Confirm($"Từ chối tin đăng '{post.Title}'?"))
                    {
                        await _postService.RejectPostAsync(post.PostID);
                        AppDialog.ShowInfo("Đã từ chối tin đăng.");
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Thao tác thất bại: " + ex.Message);
            }
        }
    }
}