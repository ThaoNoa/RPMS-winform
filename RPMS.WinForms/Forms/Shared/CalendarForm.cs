using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Calendar;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class CalendarForm : Form
    {
        private readonly ICalendarService _calendarService;
        private MonthCalendar calendar = null!;
        private ModernDataGridView dgv = null!;
        private ComboBox cboType = null!;
        private System.Collections.Generic.List<CalendarEventDto> _all = new();

        public CalendarForm(ICalendarService calendarService)
        {
            _calendarService = calendarService;
            InitializeUI();
            Load += async (s, e) => await LoadMonthAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Lịch công việc";
            ClientSize = new Size(1100, 650);

            var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 320, BackColor = AppColors.Card, Padding = new Padding(16) };
            calendar = new MonthCalendar
            {
                Location = new Point(20, 20),
                MaxSelectionCount = 1
            };
            calendar.DateChanged += async (s, e) => await LoadMonthAsync();
            calendar.DateSelected += (s, e) => FilterDay();

            cboType = new ComboBox
            {
                Location = new Point(20, 220),
                Size = new Size(260, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new object[] { "Tất cả", "Appointment", "Contract", "Maintenance", "Invoice" });
            cboType.SelectedIndex = 0;
            cboType.SelectedIndexChanged += (s, e) => FilterDay();

            var btnRefresh = new ModernButton { Text = "Làm mới", Location = new Point(20, 270), Size = new Size(120, 36) };
            btnRefresh.Click += async (s, e) => await LoadMonthAsync();

            pnlLeft.Controls.AddRange(new Control[]
            {
                new Label { Text = "Chọn ngày", Location = new Point(20, 0), AutoSize = true, ForeColor = AppColors.TextMuted },
                calendar,
                new Label { Text = "Loại sự kiện", Location = new Point(20, 200), AutoSize = true, ForeColor = AppColors.TextMuted },
                cboType,
                btnRefresh
            });

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Appointment • Contract • Maintenance • Invoice",
                Font = AppTypography.Heading,
                Location = new Point(20, 16),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = "Thời gian",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Type", HeaderText = "Loại", Width = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", Width = 260 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Detail", HeaderText = "Chi tiết", Width = 280 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", Width = 100 });

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            Controls.Add(pnlLeft);
        }

        private async System.Threading.Tasks.Task LoadMonthAsync()
        {
            try
            {
                var selected = calendar.SelectionStart;
                var from = new DateTime(selected.Year, selected.Month, 1);
                var to = from.AddMonths(1).AddDays(-1);
                var user = UserSession.CurrentUser!;
                _all = (await _calendarService.GetEventsAsync(user.UserID, user.RoleID, from, to)).ToList();
                FilterDay();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private void FilterDay()
        {
            var day = calendar.SelectionStart.Date;
            var query = _all.Where(e => e.Date.Date == day);
            if (cboType.SelectedIndex > 0)
            {
                var type = cboType.SelectedItem!.ToString();
                query = query.Where(e => e.Type == type);
            }
            dgv.DataSource = query.OrderBy(e => e.Date).ToList();
        }
    }
}
