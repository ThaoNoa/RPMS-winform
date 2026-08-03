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
            UIHelper.ApplyFormStyle(this);
            Text = "Đánh giá từ khách thuê";
            ClientSize = new Size(1100, 620);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Quản lý đánh giá",
                Font = AppTypography.Heading,
                Location = new Point(20, 16),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "HĐ", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", Width = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", Width = 280 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi", Width = 240 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ReplyCol", HeaderText = "", Text = "Phản hồi", UseColumnTextForLinkValue = true, Width = 80 });
            dgv.CellContentClick += async (s, e) => await OnCellClick(e);

            Controls.Add(dgv);
            Controls.Add(pnlTop);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var list = await _reviewService.GetByLandlordAsync(UserSession.CurrentUser!.UserID);
            dgv.DataSource = list.ToList();
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
