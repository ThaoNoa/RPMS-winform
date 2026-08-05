using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private FlowLayoutPanel flpCards = null!;
        private FlowLayoutPanel flpCharts = null!;
        private Label lblWelcome = null!;
        private LoadingPanel _loading = null!;

        public DashboardForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

            var pnlTop = UIHelper.CreatePageHeader("Dashboard");
            lblWelcome = UIHelper.GetPageHeaderTitle(pnlTop);
            lblWelcome.Font = AppTypography.Heading;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 120,
                Panel2MinSize = 120,
                BackColor = AppColors.Background
            };
            void SafeSplit()
            {
                try
                {
                    int max = split.Height - split.Panel2MinSize - split.SplitterWidth;
                    if (max > split.Panel1MinSize)
                        split.SplitterDistance = Math.Min(280, max);
                }
                catch { /* layout chưa sẵn sàng */ }
            }
            Load += (_, _) => SafeSplit();
            split.SizeChanged += (_, _) => SafeSplit();

            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                Padding = new Padding(10),
                AutoScroll = true,
                BackColor = AppColors.Background
            };

            flpCharts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                Padding = new Padding(16),
                AutoScroll = true,
                BackColor = AppColors.Background
            };

            split.Panel1.Controls.Add(flpCards);
            split.Panel2.Controls.Add(flpCharts);

            _loading = new LoadingPanel();
            Controls.Add(_loading);
            Controls.Add(split);
            Controls.Add(pnlTop);
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            var user = UserSession.CurrentUser;
            if (user == null) return;
            lblWelcome.Text = $"Tổng quan hệ thống - {user.RoleName}";
            flpCards.Controls.Clear();
            flpCharts.Controls.Clear();
            _loading.ShowLoading("Đang tải thống kê…");

            try
            {
                int roleId = user.RoleID;
                int userId = user.UserID;

                if (roleId == 1)
                {
                    var stats = await System.Threading.Tasks.Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        return await scope.ServiceProvider.GetRequiredService<IStatisticService>()
                            .GetAdminDashboardStatsAsync().ConfigureAwait(false);
                    }).ConfigureAwait(true);
                    if (IsDisposed) return;
                    AddCard("Người dùng", stats.TotalUsers.ToString(), AppColors.Primary);
                    AddCard("Nhà trọ", stats.TotalHouses.ToString(), AppColors.Success);
                    AddCard("Phòng", stats.TotalRooms.ToString(), AppColors.Primary);
                    AddCard("Tỷ lệ lấp đầy", $"{stats.OccupancyRate:0.#}%", AppColors.Success);
                    AddCard("Tin đăng", stats.TotalPosts.ToString(), AppColors.Warning);
                    AddCard("Tin chờ duyệt", stats.PendingPosts.ToString(), AppColors.Danger);
                    AddCard("Hợp đồng Active", stats.TotalActiveContracts.ToString(), AppColors.Success);
                    AddCard("Doanh thu tháng", stats.MonthlyRevenue.ToString("N0") + " đ", AppColors.Danger);
                    AddCard("Tổng doanh thu", stats.TotalRevenue.ToString("N0") + " đ", AppColors.Primary);
                    AddOccupancyChart(stats.OccupiedRooms, stats.AvailableRooms, stats.MaintenanceRooms);
                    RenderBarChart("Doanh thu 6 tháng gần nhất", stats.RevenueByMonth.Select(x => ($"T{x.Month}", x.Total)).ToList());
                    RenderList("Top Landlord (theo số phòng)", stats.TopLandlords.Select(x => $"{x.Name}: {x.Count} phòng").ToList());
                    RenderList("User theo Role", stats.UsersByRole.Select(x => $"{x.Name}: {x.Count}").ToList());
                }
                else if (roleId == 2)
                {
                    var stats = await System.Threading.Tasks.Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        return await scope.ServiceProvider.GetRequiredService<IStatisticService>()
                            .GetLandlordDashboardStatsAsync(userId).ConfigureAwait(false);
                    }).ConfigureAwait(true);
                    if (IsDisposed) return;
                    AddCard("Nhà quản lý", stats.TotalHouses.ToString(), AppColors.Primary);
                    AddCard("Tổng phòng", stats.TotalRooms.ToString(), AppColors.Primary);
                    AddCard("Đã thuê", stats.OccupiedRooms.ToString(), AppColors.Success);
                    AddCard("Chờ khách xác nhận", stats.PendingConfirmContracts.ToString(), AppColors.Warning);
                    AddCard("Trống", stats.AvailableRooms.ToString(), AppColors.Warning);
                    AddCard("Tỷ lệ lấp đầy", $"{stats.OccupancyRate:0.#}%", AppColors.Success);
                    AddCard("Lịch hẹn hôm nay", stats.TodayAppointments.ToString(), AppColors.Primary);
                    AddCard("HĐ sắp hết hạn", stats.ExpiringContracts.ToString(), AppColors.Danger);
                    AddCard("HĐ chưa thanh toán", stats.UnpaidInvoices.ToString(), AppColors.Warning);
                    AddCard("Thực thu tháng", stats.ActualCollectedRevenue.ToString("N0") + " đ", AppColors.Success);
                    AddOccupancyChart(stats.OccupiedRooms, stats.AvailableRooms, stats.MaintenanceRooms);
                    RenderBarChart("Doanh thu 6 tháng", stats.RevenueByMonth.Select(x => ($"T{x.Month}", x.Total)).ToList());
                }
                else if (roleId == 3)
                {
                    var dash = await System.Threading.Tasks.Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        return await scope.ServiceProvider.GetRequiredService<ITenantService>()
                            .GetTenantDashboardAsync(userId).ConfigureAwait(false);
                    }).ConfigureAwait(true);
                    if (IsDisposed) return;
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
                else if (roleId == 4)
                {
                    var stats = await System.Threading.Tasks.Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        return await scope.ServiceProvider.GetRequiredService<IStatisticService>()
                            .GetManagerDashboardStatsAsync(userId).ConfigureAwait(false);
                    }).ConfigureAwait(true);
                    if (IsDisposed) return;
                    AddCard("Nhà được giao", stats.ManagedHouses.ToString(), AppColors.Primary);
                    AddCard("Phòng", stats.ManagedRooms.ToString(), AppColors.Success);
                    AddCard("Bảo trì mới", stats.PendingMaintenances.ToString(), AppColors.Danger);
                    AddCard("Đang xử lý", stats.ProcessingMaintenances.ToString(), AppColors.Warning);
                    AddCard("Hóa đơn chưa trả", stats.UnpaidInvoices.ToString(), AppColors.Danger);
                    AddCard("Công việc hôm nay", stats.TodayTasks.ToString(), AppColors.Primary);
                }

                if (!IsDisposed)
                    ToastNotifier.Show(this, "Đã cập nhật thống kê", ToastKind.Success, 1800);
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Lỗi tải thống kê: " + ex.Message);
            }
            finally
            {
                if (!IsDisposed)
                    _loading.HideLoading();
            }
        }

        private void AddCard(string title, string value, Color color)
        {
            flpCards.Controls.Add(new SummaryCard { Title = title, Value = value, ThemeColor = color });
        }

        private void AddOccupancyChart(int occupied, int available, int maintenance)
        {
            var chart = new OccupancyChartPanel
            {
                ChartTitle = "Tỷ lệ lấp đầy phòng",
                Margin = new Padding(8)
            };
            chart.SetData(occupied, available, maintenance);
            flpCharts.Controls.Add(chart);
        }

        private void RenderBarChart(string title, System.Collections.Generic.List<(string Label, decimal Value)> data)
        {
            var panel = new Panel
            {
                Size = new Size(400, 220),
                BackColor = AppColors.Card,
                Margin = new Padding(8)
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
                    if (data[i].Value > 0)
                    {
                        var shortVal = data[i].Value >= 1_000_000
                            ? $"{data[i].Value / 1_000_000m:0.#}M"
                            : data[i].Value.ToString("N0");
                        g.DrawString(shortVal, captionFont, mutedBrush, x - 4, baseY - h - 16);
                    }
                }
            };
            flpCharts.Controls.Add(panel);
        }

        private void RenderList(string title, System.Collections.Generic.List<string> lines)
        {
            var lbl = new Label
            {
                Text = title + "\n\n" + string.Join("\n", lines),
                Size = new Size(260, 220),
                BackColor = AppColors.Card,
                Padding = new Padding(12),
                ForeColor = AppColors.TextMain,
                Font = AppTypography.Body,
                Margin = new Padding(8)
            };
            flpCharts.Controls.Add(lbl);
        }
    }
}
