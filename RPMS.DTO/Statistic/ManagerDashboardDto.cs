namespace RPMS.DTO.Statistic
{
    public class ManagerDashboardDto
    {
        public int ManagedHouses { get; set; }
        public int ManagedRooms { get; set; }
        public int PendingMaintenances { get; set; }
        public int ProcessingMaintenances { get; set; }
        public int UnpaidInvoices { get; set; }
        public int TodayTasks { get; set; }
    }
}
