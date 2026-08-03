using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class ReportForm : Form
    {
        private readonly IReportService _reportService;
        private FlowLayoutPanel flpCards = null!;
        private Panel pnlBody = null!;
        private Label lblTitle = null!;

        public ReportForm(IReportService reportService)
        {
            _reportService = reportService;
            InitializeUI();
            Load += async (s, e) => await LoadReportAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Báo cáo & xuất dữ liệu";
            ClientSize = new Size(1100, 700);
            AutoScroll = false;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = AppColors.Card };
            lblTitle = new Label
            {
                Text = "Báo cáo tổng hợp",
                Font = AppTypography.Heading,
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            };
            var btnCsv = new ModernButton { Text = "Xuất Excel (CSV)", Location = new Point(700, 16), Size = new Size(160, 36), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnCsv.Click += async (s, e) => await ExportCsvAsync();
            var btnHtml = new ModernButton { Text = "Xuất PDF/HTML", Location = new Point(880, 16), Size = new Size(150, 36), BackColor = AppColors.Success, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnHtml.Click += async (s, e) => await ExportHtmlAsync();
            pnlTop.Controls.AddRange(new Control[] { lblTitle, btnCsv, btnHtml });

            var pnlScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppColors.Background };

            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(12),
                AutoScroll = false,
                BackColor = AppColors.Background
            };
            pnlBody = new Panel { Dock = DockStyle.Top, AutoScroll = true, Padding = new Padding(16), MinimumSize = new Size(0, 320), BackColor = AppColors.Background };

            pnlScroll.Controls.Add(pnlBody);
            pnlScroll.Controls.Add(flpCards);

            Controls.Add(pnlScroll);
            Controls.Add(pnlTop);
        }

        private async System.Threading.Tasks.Task LoadReportAsync()
        {
            try
            {
                var user = UserSession.CurrentUser!;
                var report = user.RoleID == 1
                    ? await _reportService.GetAdminReportAsync()
                    : await _reportService.GetLandlordReportAsync(user.UserID);

                flpCards.Controls.Clear();
                flpCards.Controls.Add(new SummaryCard { Title = "Doanh thu tháng", Value = report.MonthlyRevenue.ToString("N0") + " đ", ThemeColor = AppColors.Primary });
                flpCards.Controls.Add(new SummaryCard { Title = "Tổng doanh thu", Value = report.TotalRevenue.ToString("N0") + " đ", ThemeColor = AppColors.Success });
                flpCards.Controls.Add(new SummaryCard { Title = "HĐ Active", Value = report.ActiveContracts.ToString(), ThemeColor = AppColors.Warning });
                flpCards.Controls.Add(new SummaryCard { Title = "Tỷ lệ thuê", Value = report.OccupancyRate.ToString("0.0") + "%", ThemeColor = AppColors.Danger });

                pnlBody.Controls.Clear();
                int y = 10;
                y = AddSection(pnlBody, "Doanh thu 6 tháng", report.RevenueByMonth.Select(x => $"Tháng {x.Month}: {x.Total:N0} đ"), y);
                y = AddSection(pnlBody, "Top phòng", report.TopRooms.Select(x => $"{x.Name}: {x.Count} HĐ"), y);
                y = AddSection(pnlBody, "Top chủ nhà", report.TopLandlords.Select(x => $"{x.Name}: {x.Count} phòng"), y);
                AddSection(pnlBody, "Top người thuê", report.TopTenants.Select(x => $"{x.Name}: {x.Amount:N0} đ/tháng"), y);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private static int AddSection(Control parent, string title, System.Collections.Generic.IEnumerable<string> lines, int y)
        {
            var box = new Label
            {
                Text = title + "\n\n" + string.Join("\n", lines.DefaultIfEmpty("(không có dữ liệu)")),
                Location = new Point(20, y),
                Size = new Size(Math.Max(400, parent.ClientSize.Width - 56), 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = AppColors.Card,
                Padding = new Padding(12),
                ForeColor = AppColors.TextMain,
                Font = AppTypography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(box);
            return y + 150;
        }

        private async System.Threading.Tasks.Task ExportCsvAsync()
        {
            try
            {
                var user = UserSession.CurrentUser!;
                var report = user.RoleID == 1
                    ? await _reportService.GetAdminReportAsync()
                    : await _reportService.GetLandlordReportAsync(user.UserID);

                if (!ExportHelper.SaveFile("CSV (*.csv)|*.csv", $"RPMS_Report_{DateTime.Now:yyyyMMdd}.csv", out var path))
                    return;

                var rows = report.RevenueByMonth.Select(x => new[]
                {
                    x.Month.ToString(),
                    x.Total.ToString("0")
                });
                ExportHelper.ExportCsv(path,
                    new[] { "Month", "Revenue" },
                    rows);
                AppDialog.ShowInfo("Đã xuất CSV (mở bằng Excel):\n" + path);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Xuất CSV thất bại: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task ExportHtmlAsync()
        {
            try
            {
                var user = UserSession.CurrentUser!;
                var report = user.RoleID == 1
                    ? await _reportService.GetAdminReportAsync()
                    : await _reportService.GetLandlordReportAsync(user.UserID);

                if (!ExportHelper.SaveFile("HTML (*.html)|*.html", $"RPMS_Report_{DateTime.Now:yyyyMMdd}.html", out var path))
                    return;

                var sb = new StringBuilder();
                sb.Append("<div class='card'><h3>Tóm tắt</h3><ul>");
                sb.Append($"<li>Doanh thu tháng: {report.MonthlyRevenue:N0} đ</li>");
                sb.Append($"<li>Tổng doanh thu: {report.TotalRevenue:N0} đ</li>");
                sb.Append($"<li>HĐ Active: {report.ActiveContracts}</li>");
                sb.Append($"<li>Tỷ lệ thuê: {report.OccupancyRate}%</li></ul></div>");
                sb.Append("<div class='card'><h3>Doanh thu theo tháng</h3><table><tr><th>Tháng</th><th>Doanh thu</th></tr>");
                foreach (var m in report.RevenueByMonth)
                    sb.Append($"<tr><td>{m.Month}</td><td>{m.Total:N0}</td></tr>");
                sb.Append("</table></div>");

                ExportHelper.ExportHtmlReport(path, "RPMS Report", sb.ToString());
                AppDialog.ShowInfo("Đã xuất HTML (in ra PDF từ trình duyệt):\n" + path);
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Xuất HTML thất bại: " + ex.Message);
            }
        }
    }
}
