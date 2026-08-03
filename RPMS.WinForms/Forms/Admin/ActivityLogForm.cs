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
    public class ActivityLogForm : Form
    {
        private readonly IActivityLogService _activityLogService;
        private ModernDataGridView dgv = null!;

        public ActivityLogForm(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Nhật ký hệ thống";
            ClientSize = new Size(1050, 620);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Activity Log",
                Font = AppTypography.Heading,
                Location = new Point(20, 16),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });
            var btnRefresh = new ModernButton
            {
                Text = "Làm mới",
                Location = new Point(900, 12),
                Size = new Size(110, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlTop.Controls.Add(btnRefresh);

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LogID", HeaderText = "ID", Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "Người dùng", Width = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "Hành động", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Details", HeaderText = "Chi tiết", Width = 420 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
            MinimumSize = new Size(700, 480);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var logs = await _activityLogService.GetRecentAsync(200);
                dgv.DataSource = logs.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
