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
            Text = "Đánh giá sau thuê";
            ClientSize = new Size(1100, 650);

            var header = UIHelper.CreatePageHeader("Đánh giá sau thuê");

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 120,
                Panel2MinSize = 120
            };
            void SafeSplit()
            {
                try
                {
                    int max = split.Height - split.Panel2MinSize - split.SplitterWidth;
                    if (max > split.Panel1MinSize)
                        split.SplitterDistance = Math.Min(300, max);
                }
                catch { }
            }
            Load += (_, _) => SafeSplit();
            split.SizeChanged += (_, _) => SafeSplit();

            dgvContracts = new ModernDataGridView();
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", FillWeight = 20 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 15 });
            dgvContracts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 15 });
            dgvContracts.Columns.Add(new DataGridViewLinkColumn { Name = "SelectCol", HeaderText = "", Text = "Chọn", UseColumnTextForLinkValue = true, FillWeight = 10 });
            dgvContracts.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || dgvContracts.Columns[e.ColumnIndex].Name != "SelectCol") return;
                if (dgvContracts.Rows[e.RowIndex].DataBoundItem is not RPMS.DTO.Contract.ContractDto row) return;
                _selectedContractId = row.ContractID;
                AppDialog.ShowInfo($"Đã chọn hợp đồng {row.ContractCode} để đánh giá.");
            };
            UIHelper.ApplyGridFill(dgvContracts);

            numRating = new NumericUpDown { Minimum = 1, Maximum = 5, Value = 5, Width = 80, Font = AppTypography.Body };
            txtComment = new TextBox
            {
                Multiline = true,
                Height = 50,
                Width = 420,
                Font = AppTypography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 18)
            };

            var commentField = new Panel
            {
                Width = 420,
                Height = 72,
                Margin = new Padding(0, 0, AppLayout.FieldGap, 6)
            };
            commentField.Controls.Add(new Label
            {
                Text = "Nhận xét",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            commentField.Controls.Add(txtComment);

            var btnSubmit = UIHelper.PrimaryButton("Gửi đánh giá", 140);
            btnSubmit.Margin = new Padding(0, 28, AppLayout.FieldGap, 6);
            btnSubmit.Click += async (s, e) => await SubmitAsync();

            var createBar = UIHelper.CreateFilterBar();
            createBar.Controls.Add(UIHelper.CreateLabeledField("Số sao", numRating, 90));
            createBar.Controls.Add(commentField);
            createBar.Controls.Add(btnSubmit);

            var topPanel = new Panel { Dock = DockStyle.Fill };
            topPanel.Controls.Add(dgvContracts);
            topPanel.Controls.Add(createBar);
            split.Panel1.Controls.Add(topPanel);

            dgvReviews = new ModernDataGridView();
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "HĐ", FillWeight = 14 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rating", HeaderText = "Sao", FillWeight = 8 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Nhận xét", FillWeight = 39 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LandlordReply", HeaderText = "Phản hồi chủ nhà", FillWeight = 39 });
            UIHelper.ApplyGridFill(dgvReviews);
            split.Panel2.Controls.Add(dgvReviews);

            Controls.Add(split);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, split);
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
