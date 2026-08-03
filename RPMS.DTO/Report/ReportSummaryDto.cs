using System.Collections.Generic;
using RPMS.DTO.Statistic;

namespace RPMS.DTO.Report
{
    public class ReportSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public double OccupancyRate { get; set; }
        public List<RevenueChartData> RevenueByMonth { get; set; } = new();
        public List<NamedCountDto> TopRooms { get; set; } = new();
        public List<NamedCountDto> TopLandlords { get; set; } = new();
        public List<NamedCountDto> TopTenants { get; set; } = new();
    }
}
