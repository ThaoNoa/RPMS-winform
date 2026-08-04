using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Review;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordReviewForm : Form
    {
        private readonly IReviewService _reviewService;
        private ModernDataGridView dgv = null!;

        public LandlordReviewForm(IReviewService reviewService)
        {
            _reviewService = reviewService;
            InitializeUI();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeUI()
        {
            Text = "Đánh giá từ khách thuê";
            ClientSize = new Size(1100, 620);

            var header = UIHelper.CreatePageHeader("Quản lý đánh giá");

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "HĐ", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", FillWeight = 28 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi", FillWeight = 24 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ReplyCol", HeaderText = "", Text = "Phản hồi", UseColumnTextForLinkValue = true, FillWeight = 8 });
            dgv.CellContentClick += async (s, e) => await OnCellClick(e);

            Controls.Add(dgv);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                var list = await _reviewService.GetByLandlordAsync(UserSession.CurrentUser!.UserID);
                if (IsDisposed) return;
                dgv.DataSource = list.ToList();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải đánh giá: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task OnCellClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "ReplyCol") return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as ReviewDto;
            if (item == null) return;

            var reply = AppDialog.Prompt("Nhập phản hồi của bạn:", "Phản hồi đánh giá", item.LandlordReply ?? "");
            if (reply == null) return;

            try
            {
                await _reviewService.ReplyAsync(UserSession.CurrentUser!.UserID, new ReplyReviewDto
                {
                    ReviewID = item.ReviewID,
                    Reply = reply
                });
                AppDialog.ShowInfo("Đã gửi phản hồi.");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
