using System;
using System.Collections.Generic;

namespace RPMS.DTO.Statistic
{
    public class LandlordDashboardDto
    {
        public int TotalHouses { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public double OccupancyRate { get; set; }
        public int TodayAppointments { get; set; }
        public int ExpiringContracts { get; set; }
        public int UnpaidInvoices { get; set; }
        public decimal ExpectedMonthlyRevenue { get; set; }
        public decimal ActualCollectedRevenue { get; set; }
        public int PendingMaintenanceRequests { get; set; }
        public List<RevenueChartData> RevenueByMonth { get; set; } = new();
    }
}
