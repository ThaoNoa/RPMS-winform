using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Dashboard
{
    public class DashboardForm : Form
    {
        private readonly IStatisticService _statisticService;
        private readonly ITenantService _tenantService;
        private FlowLayoutPanel flpCards = null!;
        private Panel pnlCharts = null!;
        private Label lblWelcome = null!;

        public DashboardForm(IStatisticService statisticService, ITenantService tenantService)
        {
            _statisticService = statisticService;
            _tenantService = tenantService;
            InitializeUI();
            Load += DashboardForm_Load!;
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            MinimumSize = new Size(900, 560);
            ClientSize = new Size(1100, 700);
            BackColor = AppColors.Background;
            Text = "Dashboard";
            AutoScroll = false;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = AppColors.Background };
            lblWelcome = new Label
            {
                Font = AppTypography.Subtitle,
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlTop.Controls.Add(lblWelcome);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300,
                BackColor = AppColors.Background
            };

            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                Padding = new Padding(10),
                AutoScroll = true,
                BackColor = AppColors.Background
            };

            pnlCharts = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = AppColors.Background,
                AutoScroll = true
            };

            split.Panel1.Controls.Add(flpCards);
            split.Panel2.Controls.Add(pnlCharts);

            Controls.Add(split);
            Controls.Add(pnlTop);
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            var user = UserSession.CurrentUser;
            if (user == null) return;
            lblWelcome.Text = $"Tổng quan hệ thống - {user.RoleName}";
            flpCards.Controls.Clear();
            pnlCharts.Controls.Clear();

            try
            {
                if (user.RoleID == 1)
                {
                    var stats = await _statisticService.GetAdminDashboardStatsAsync();
                    AddCard("Người dùng", stats.TotalUsers.ToString(), AppColors.Primary);
                    AddCard("Nhà trọ", stats.TotalHouses.ToString(), AppColors.Success);
                    AddCard("Phòng", stats.TotalRooms.ToString(), AppColors.Primary);
                    AddCard("Tin đăng", stats.TotalPosts.ToString(), AppColors.Warning);
                    AddCard("Tin chờ duyệt", stats.PendingPosts.ToString(), AppColors.Danger);
                    AddCard("Hợp đồng Active", stats.TotalActiveContracts.ToString(), AppColors.Success);
                    AddCard("Doanh thu tháng", stats.MonthlyRevenue.ToString("N0") + " đ", AppColors.Danger);
                    AddCard("Tổng doanh thu", stats.TotalRevenue.ToString("N0") + " đ", AppColors.Primary);
                    RenderBarChart("Doanh thu 6 tháng gần nhất", stats.RevenueByMonth.Select(x => ($"T{x.Month}", x.Total)).ToList());
                    RenderList("Top Landlord (theo số phòng)", stats.TopLandlords.Select(x => $"{x.Name}: {x.Count} phòng").ToList(), 420);
                    RenderList("User theo Role", stats.UsersByRole.Select(x => $"{x.Name}: {x.Count}").ToList(), 700);
                }
                else if (user.RoleID == 2)
                {
                    var stats = await _statisticService.GetLandlordDashboardStatsAsync(user.UserID);
                    AddCard("Nhà quản lý", stats.TotalHouses.ToString(), AppColors.Primary);
                    AddCard("Tổng phòng", stats.TotalRooms.ToString(), AppColors.Primary);
                    AddCard("Đã thuê", stats.OccupiedRooms.ToString(), AppColors.Success);
                    AddCard("Trống", stats.AvailableRooms.ToString(), AppColors.Warning);
                    AddCard("Lịch hẹn hôm nay", stats.TodayAppointments.ToString(), AppColors.Primary);
                    AddCard("HĐ sắp hết hạn", stats.ExpiringContracts.ToString(), AppColors.Danger);
                    AddCard("HĐ chưa thanh toán", stats.UnpaidInvoices.ToString(), AppColors.Warning);
                    AddCard("Thực thu tháng", stats.ActualCollectedRevenue.ToString("N0") + " đ", AppColors.Success);
                    RenderBarChart("Doanh thu 6 tháng", stats.RevenueByMonth.Select(x => ($"T{x.Month}", x.Total)).ToList());
                }
                else if (user.RoleID == 3)
                {
                    var dash = await _tenantService.GetTenantDashboardAsync(user.UserID);
                    string roomInfo = dash.CurrentContract != null
                        ? $"{dash.CurrentContract.RoomNumber}"
                        : "Chưa thuê";
                    AddCard("Phòng đang thuê", roomInfo, AppColors.Primary);
                    AddCard("Hóa đơn chưa trả", (dash.UnpaidInvoices?.Count ?? 0).ToString(), AppColors.Danger);
                    AddCard("Lịch hẹn sắp tới", (dash.UpcomingAppointments?.Count ?? 0).ToString(), AppColors.Warning);
                    AddCard("Yêu thích", dash.FavoriteCount.ToString(), AppColors.Success);
                    AddCard("Thông báo mới", (dash.RecentNotifications?.Count(n => !n.IsRead) ?? 0).ToString(), AppColors.Primary);
                    AddCard("Bảo trì gần đây", (dash.RecentMaintenances?.Count ?? 0).ToString(), AppColors.Warning);
                }
                else if (user.RoleID == 4)
                {
                    var stats = await _statisticService.GetManagerDashboardStatsAsync(user.UserID);
                    AddCard("Nhà được giao", stats.ManagedHouses.ToString(), AppColors.Primary);
                    AddCard("Phòng", stats.ManagedRooms.ToString(), AppColors.Success);
                    AddCard("Bảo trì mới", stats.PendingMaintenances.ToString(), AppColors.Danger);
                    AddCard("Đang xử lý", stats.ProcessingMaintenances.ToString(), AppColors.Warning);
                    AddCard("Hóa đơn chưa trả", stats.UnpaidInvoices.ToString(), AppColors.Danger);
                    AddCard("Công việc hôm nay", stats.TodayTasks.ToString(), AppColors.Primary);
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải thống kê: " + ex.Message);
            }
        }

        private void AddCard(string title, string value, Color color)
        {
            flpCards.Controls.Add(new SummaryCard { Title = title, Value = value, ThemeColor = color });
        }

        private void RenderBarChart(string title, System.Collections.Generic.List<(string Label, decimal Value)> data)
        {
            var panel = new Panel
            {
                Location = new Point(20, 10),
                Size = new Size(380, 220),
                BackColor = AppColors.Card
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.Clear(AppColors.Card);
                using var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                using var captionFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                using var titleBrush = new SolidBrush(AppColors.TextMain);
                using var mutedBrush = new SolidBrush(AppColors.TextMuted);
                g.DrawString(title, titleFont, titleBrush, 12, 10);
                if (data.Count == 0) return;
                decimal max = Math.Max(1, data.Max(x => x.Value));
                int baseY = 190;
                int barWidth = 40;
                int gap = 18;
                int startX = 24;
                for (int i = 0; i < data.Count; i++)
                {
                    int h = (int)(120m * (data[i].Value / max));
                    int x = startX + i * (barWidth + gap);
                    using var brush = new SolidBrush(AppColors.Primary);
                    g.FillRectangle(brush, x, baseY - h, barWidth, h);
                    g.DrawString(data[i].Label, captionFont, mutedBrush, x, baseY + 4);
                }
            };
            pnlCharts.Controls.Add(panel);
        }

        private void RenderList(string title, System.Collections.Generic.List<string> lines, int x)
        {
            var lbl = new Label
            {
                Text = title + "\n\n" + string.Join("\n", lines),
                Location = new Point(x, 10),
                Size = new Size(250, 220),
                BackColor = AppColors.Card,
                Padding = new Padding(12),
                ForeColor = AppColors.TextMain,
                Font = AppTypography.Body
            };
            pnlCharts.Controls.Add(lbl);
        }
    }
}
