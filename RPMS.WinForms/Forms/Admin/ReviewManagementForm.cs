using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    public class ReviewManagementForm : Form
    {
        private readonly IReviewService _reviewService;
        private ModernDataGridView dgv = null!;

        public ReviewManagementForm(IReviewService reviewService)
        {
            _reviewService = reviewService;
            InitializeUI();
            Load += async (s, e) =>
            {
                try
                {
                    dgv.DataSource = (await _reviewService.GetAllAsync()).ToList();
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError(ex.Message);
                }
            };
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Quản lý đánh giá";
            ClientSize = new Size(1100, 620);
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Tất cả đánh giá",
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
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordName", HeaderText = "Chủ nhà", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", Width = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", Width = 260 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi", Width = 220 });
            Controls.Add(dgv);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
            MinimumSize = new Size(700, 480);
        }
    }
}
