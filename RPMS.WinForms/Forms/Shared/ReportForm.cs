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
        private FlowLayoutPanel flpSections = null!;

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

            var btnCsv = UIHelper.PrimaryButton("Xuất Excel (CSV)", 160);
            btnCsv.Click += async (s, e) => await ExportCsvAsync();
            var btnHtml = UIHelper.PrimaryButton("Xuất PDF/HTML", 150);
            btnHtml.BackColor = AppColors.Success;
            btnHtml.Click += async (s, e) => await ExportHtmlAsync();

            var header = UIHelper.CreatePageHeader("Báo cáo tổng hợp", btnCsv, btnHtml);

            var pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppColors.Background
            };

            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(AppLayout.PagePadding),
                AutoScroll = false,
                BackColor = AppColors.Background
            };

            flpSections = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(AppLayout.PagePadding),
                BackColor = AppColors.Background
            };
            flpSections.Resize += (s, e) => SyncSectionWidths();

            pnlScroll.Controls.Add(flpSections);
            pnlScroll.Controls.Add(flpCards);

            Controls.Add(pnlScroll);
            Controls.Add(header);
        }

        private void SyncSectionWidths()
        {
            int w = Math.Max(400, flpSections.ClientSize.Width - 8);
            foreach (Control c in flpSections.Controls)
                c.Width = w;
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

                flpSections.Controls.Clear();
                AddSection(flpSections, "Doanh thu 6 tháng", report.RevenueByMonth.Select(x => $"Tháng {x.Month}: {x.Total:N0} đ"));
                AddSection(flpSections, "Top phòng", report.TopRooms.Select(x => $"{x.Name}: {x.Count} HĐ"));
                AddSection(flpSections, "Top chủ nhà", report.TopLandlords.Select(x => $"{x.Name}: {x.Count} phòng"));
                AddSection(flpSections, "Top người thuê", report.TopTenants.Select(x => $"{x.Name}: {x.Amount:N0} đ/tháng"));
                SyncSectionWidths();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private static void AddSection(FlowLayoutPanel parent, string title, System.Collections.Generic.IEnumerable<string> lines)
        {
            var box = new Label
            {
                Text = title + "\n\n" + string.Join("\n", lines.DefaultIfEmpty("(không có dữ liệu)")),
                Size = new Size(Math.Max(400, parent.ClientSize.Width - 8), 140),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = AppColors.Card,
                Padding = new Padding(12),
                ForeColor = AppColors.TextMain,
                Font = AppTypography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(box);
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
