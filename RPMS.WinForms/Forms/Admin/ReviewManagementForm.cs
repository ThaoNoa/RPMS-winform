using RPMS.BLL.Interfaces;
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
            Text = "Quản lý đánh giá";
            ClientSize = new Size(1100, 620);

            var header = UIHelper.CreatePageHeader("Tất cả đánh giá");

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "HĐ", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordName", HeaderText = "Chủ nhà", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", FillWeight = 26 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi", FillWeight = 20 });

            Controls.Add(dgv);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
        }
    }
}
