using RPMS.BLL.Interfaces;
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
            Text = "Nhật ký hệ thống";
            ClientSize = new Size(1050, 620);

            var btnRefresh = UIHelper.SecondaryButton("Làm mới");
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            var header = UIHelper.CreatePageHeader("Activity Log", btnRefresh);

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LogID", HeaderText = "ID", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "Người dùng", FillWeight = 16 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "Hành động", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Details", HeaderText = "Chi tiết", FillWeight = 42 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                FillWeight = 16,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            Controls.Add(dgv);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
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
