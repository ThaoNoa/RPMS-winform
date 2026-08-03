using RPMS.BLL.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatisticService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardStatsAsync()
        {
            var now = DateTime.Now;
            var totalUsers = await _unitOfWork.Users.CountAsync(u => u.Status == "Active");
            var totalHouses = await _unitOfWork.Houses.CountAsync(h => h.Status == "Active");
            var totalRooms = await _unitOfWork.Rooms.CountAsync(_ => true);
            var totalPosts = await _unitOfWork.Posts.CountAsync(_ => true);
            var pendingPosts = await _unitOfWork.Posts.CountAsync(p => p.Status == "Pending");
            var activeContracts = await _unitOfWork.Contracts.CountAsync(c => c.Status == "Active");

            var paidThisMonth = await _unitOfWork.Invoices.FindAsync(i =>
                i.Status == "Paid" &&
                i.PaidDate != null &&
                i.PaidDate.Value.Month == now.Month &&
                i.PaidDate.Value.Year == now.Year);
            var allPaid = await _unitOfWork.Invoices.FindAsync(i => i.Status == "Paid");

            var revenueByMonth = new List<RevenueChartData>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var total = allPaid
                    .Where(x => x.PaidDate != null && x.PaidDate.Value.Month == month.Month && x.PaidDate.Value.Year == month.Year)
                    .Sum(x => x.Total);
                revenueByMonth.Add(new RevenueChartData { Month = month.Month, Total = total });
            }

            var houses = await _unitOfWork.Houses.GetAllAsync("Owner, Rooms");
            var topLandlords = houses
                .GroupBy(h => new { h.OwnerID, Name = h.Owner?.FullName ?? ("User#" + h.OwnerID) })
                .Select(g => new NamedCountDto
                {
                    Name = g.Key.Name,
                    Count = g.Sum(x => x.Rooms?.Count ?? 0),
                    Amount = 0
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var users = await _unitOfWork.Users.GetAllAsync("Role");
            var usersByRole = users
                .GroupBy(u => u.Role?.RoleName ?? "Unknown")
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalHouses = totalHouses,
                TotalRooms = totalRooms,
                TotalPosts = totalPosts,
                PendingPosts = pendingPosts,
                TotalActiveContracts = activeContracts,
                MonthlyRevenue = paidThisMonth.Sum(i => i.Total),
                TotalRevenue = allPaid.Sum(i => i.Total),
                RevenueByMonth = revenueByMonth,
                TopLandlords = topLandlords,
                UsersByRole = usersByRole
            };
        }

        public async Task<LandlordDashboardDto> GetLandlordDashboardStatsAsync(int landlordId)
        {
            var now = DateTime.Now;
            var houseEntities = (await _unitOfWork.Houses.FindAsync(h => h.OwnerID == landlordId)).ToList();
            var houses = houseEntities.Select(h => h.HouseID).ToList();
            var allRooms = await _unitOfWork.Rooms.FindAsync(r => houses.Contains(r.HouseID));
            int totalRooms = allRooms.Count();
            int occupiedRooms = allRooms.Count(r => r.Status == "Occupied");
            int availableRooms = allRooms.Count(r => r.Status == "Available");

            var contracts = await _unitOfWork.Contracts.FindAsync(
                c => c.Status == "Active" && houses.Contains(c.Room.HouseID),
                "Room");
            decimal expectedRevenue = contracts.Sum(c => c.MonthlyRent);

            var paidInvoices = await _unitOfWork.Invoices.FindAsync(
                i => i.Status == "Paid" &&
                     i.PaidDate != null &&
                     i.PaidDate.Value.Month == now.Month &&
                     i.PaidDate.Value.Year == now.Year &&
                     houses.Contains(i.Contract.Room.HouseID),
                "Contract.Room");
            decimal actualRevenue = paidInvoices.Sum(i => i.Total);

            var pendingRequests = await _unitOfWork.MaintenanceRequests.CountAsync(
                m => m.Status == "Pending" && houses.Contains(m.Contract.Room.HouseID));

            var today = now.Date;
            var appointments = await _unitOfWork.Appointments.FindAsync(
                a => houses.Contains(a.Room.HouseID) && a.AppointmentDate.Date == today,
                "Room");

            var expiring = contracts.Count(c => c.EndDate.Date <= today.AddDays(30));
            var unpaid = await _unitOfWork.Invoices.CountAsync(
                i => i.Status == "Unpaid" && houses.Contains(i.Contract.Room.HouseID));

            var allPaid = await _unitOfWork.Invoices.FindAsync(
                i => i.Status == "Paid" && houses.Contains(i.Contract.Room.HouseID),
                "Contract.Room");
            var revenueByMonth = new List<RevenueChartData>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var total = allPaid
                    .Where(x => x.PaidDate != null && x.PaidDate.Value.Month == month.Month && x.PaidDate.Value.Year == month.Year)
                    .Sum(x => x.Total);
                revenueByMonth.Add(new RevenueChartData { Month = month.Month, Total = total });
            }

            return new LandlordDashboardDto
            {
                TotalHouses = houses.Count,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                AvailableRooms = availableRooms,
                TodayAppointments = appointments.Count(),
                ExpiringContracts = expiring,
                UnpaidInvoices = unpaid,
                ExpectedMonthlyRevenue = expectedRevenue,
                ActualCollectedRevenue = actualRevenue,
                PendingMaintenanceRequests = pendingRequests,
                RevenueByMonth = revenueByMonth
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardStatsAsync(int managerId)
        {
            var assignments = await _unitOfWork.Assignments.FindAsync(a => a.ManagerID == managerId && a.Status == "Active");
            var houseIds = assignments.Select(a => a.HouseID).ToList();

            var rooms = await _unitOfWork.Rooms.CountAsync(r => houseIds.Contains(r.HouseID));
            var pending = await _unitOfWork.MaintenanceRequests.CountAsync(
                m => houseIds.Contains(m.Contract.Room.HouseID) && m.Status == "Pending");
            var processing = await _unitOfWork.MaintenanceRequests.CountAsync(
                m => houseIds.Contains(m.Contract.Room.HouseID) && m.Status == "Processing");
            var unpaid = await _unitOfWork.Invoices.CountAsync(
                i => i.Status == "Unpaid" && houseIds.Contains(i.Contract.Room.HouseID));

            return new ManagerDashboardDto
            {
                ManagedHouses = houseIds.Count,
                ManagedRooms = rooms,
                PendingMaintenances = pending,
                ProcessingMaintenances = processing,
                UnpaidInvoices = unpaid,
                TodayTasks = pending + processing
            };
        }
    }
}
