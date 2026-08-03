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

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantReviewForm : Form
    {
        private readonly IReviewService _reviewService;
        private readonly IContractService _contractService;
        private ModernDataGridView dgvContracts = null!;
        private ModernDataGridView dgvReviews = null!;
        private NumericUpDown numRating = null!;
        private TextBox txtComment = null!;
        private int _selectedContractId;

        public TenantReviewForm(IReviewService reviewService, IContractService contractService)
        {
            _reviewService = reviewService;
            _contractService = contractService;
            InitializeUI();
            Load += async (s, e) => await ReloadAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Đánh giá sau thuê";
            ClientSize = new Size(1100, 650);
            MinimumSize = new Size(900, 480);
            AutoScroll = false;

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 280 };

            dgvContracts = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvContracts.AutoGenerateColumns = false;
            dgvContracts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", Width = 140 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 100 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", Width = 100 });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "SelectCol", HeaderText = "", Text = "Chọn", UseColumnTextForLinkValue = true, Width = 60 });
            dgvContracts.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || dgvContracts.Columns[e.ColumnIndex].Name != "SelectCol") return;
                if (dgvContracts.Rows[e.RowIndex].DataBoundItem is not RPMS.DTO.Contract.ContractDto row) return;
                _selectedContractId = row.ContractID;
                AppDialog.ShowInfo($"Đã chọn hợp đồng {row.ContractCode} để đánh giá.");
            };

            var pnlCreate = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = AppColors.Card };
            pnlCreate.Controls.Add(new Label { Text = "Số sao", Location = new Point(20, 20), AutoSize = true });
            numRating = new NumericUpDown { Location = new Point(80, 18), Minimum = 1, Maximum = 5, Value = 5, Width = 60 };
            pnlCreate.Controls.Add(numRating);
            pnlCreate.Controls.Add(new Label { Text = "Nhận xét", Location = new Point(160, 20), AutoSize = true });
            txtComment = new TextBox { Location = new Point(230, 18), Size = new Size(620, 50), Multiline = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlCreate.Controls.Add(txtComment);
            var btnSubmit = new ModernButton { Text = "Gửi đánh giá", Location = new Point(870, 25), Size = new Size(140, 40), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnSubmit.Click += async (s, e) => await SubmitAsync();
            pnlCreate.Controls.Add(btnSubmit);

            var topPanel = new Panel { Dock = DockStyle.Fill };
            topPanel.Controls.Add(dgvContracts);
            topPanel.Controls.Add(pnlCreate);
            split.Panel1.Controls.Add(topPanel);

            dgvReviews = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvReviews.AutoGenerateColumns = false;
            dgvReviews.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "HĐ", Width = 120 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", Width = 50 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", Width = 320 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi chủ nhà", Width = 320 });
            split.Panel2.Controls.Add(dgvReviews);

            Controls.Add(split);
        }

        private async System.Threading.Tasks.Task ReloadAsync()
        {
            var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
            dgvContracts.DataSource = contracts
                .Where(c => c.Status == "Terminated" || c.Status == "Expired")
                .ToList();
            var reviews = await _reviewService.GetByTenantAsync(UserSession.CurrentUser.UserID);
            dgvReviews.DataSource = reviews.ToList();
        }

        private async System.Threading.Tasks.Task SubmitAsync()
        {
            if (_selectedContractId <= 0)
            {
                AppDialog.ShowWarning("Vui lòng chọn hợp đồng đã kết thúc để đánh giá.");
                return;
            }
            try
            {
                await _reviewService.CreateReviewAsync(UserSession.CurrentUser!.UserID, new CreateReviewDto
                {
                    ContractID = _selectedContractId,
                    Rating = (int)numRating.Value,
                    Comment = txtComment.Text.Trim()
                });
                AppDialog.ShowInfo("Gửi đánh giá thành công.");
                txtComment.Text = "";
                _selectedContractId = 0;
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
