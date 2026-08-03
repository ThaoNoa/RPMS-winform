using System.Collections.Generic;

namespace RPMS.DTO.Statistic
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalHouses { get; set; }
        public int TotalRooms { get; set; }
        public int TotalPosts { get; set; }
        public int PendingPosts { get; set; }
        public int TotalActiveContracts { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RevenueChartData> RevenueByMonth { get; set; } = new();
        public List<NamedCountDto> TopLandlords { get; set; } = new();
        public List<NamedCountDto> UsersByRole { get; set; } = new();
    }

    public class NamedCountDto
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
