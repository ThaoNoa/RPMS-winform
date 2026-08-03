using RPMS.BLL.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Report;
using RPMS.DTO.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReportSummaryDto> GetAdminReportAsync()
        {
            var now = DateTime.Now;
            var rooms = (await _unitOfWork.Rooms.GetAllAsync()).ToList();
            var contracts = (await _unitOfWork.Contracts.GetAllAsync("Room, Tenant")).ToList();
            var paid = (await _unitOfWork.Invoices.FindAsync(i => i.Status == "Paid")).ToList();

            var report = BuildBase(rooms, contracts, paid, now);
            var houses = await _unitOfWork.Houses.GetAllAsync("Owner, Rooms");
            report.TopLandlords = houses
                .GroupBy(h => h.Owner?.FullName ?? ("#" + h.OwnerID))
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Sum(x => x.Rooms?.Count ?? 0) })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            report.TopTenants = contracts
                .Where(c => c.Status == "Active")
                .GroupBy(c => c.Tenant?.FullName ?? ("#" + c.TenantID))
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count(), Amount = g.Sum(x => x.MonthlyRent) })
                .OrderByDescending(x => x.Amount).Take(5).ToList();

            report.TopRooms = contracts
                .GroupBy(c => c.Room?.RoomNumber ?? ("Room#" + c.RoomID))
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            return report;
        }

        public async Task<ReportSummaryDto> GetLandlordReportAsync(int landlordId)
        {
            var now = DateTime.Now;
            var houseIds = (await _unitOfWork.Houses.FindAsync(h => h.OwnerID == landlordId)).Select(h => h.HouseID).ToList();
            var rooms = (await _unitOfWork.Rooms.FindAsync(r => houseIds.Contains(r.HouseID))).ToList();
            var contracts = (await _unitOfWork.Contracts.FindAsync(
                c => houseIds.Contains(c.Room.HouseID), "Room, Tenant")).ToList();
            var paid = (await _unitOfWork.Invoices.FindAsync(
                i => i.Status == "Paid" && houseIds.Contains(i.Contract.Room.HouseID),
                "Contract.Room")).ToList();

            var report = BuildBase(rooms, contracts, paid, now);
            report.TopTenants = contracts
                .Where(c => c.Status == "Active")
                .GroupBy(c => c.Tenant?.FullName ?? ("#" + c.TenantID))
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count(), Amount = g.Sum(x => x.MonthlyRent) })
                .OrderByDescending(x => x.Amount).Take(5).ToList();
            report.TopRooms = contracts
                .GroupBy(c => c.Room?.RoomNumber ?? ("Room#" + c.RoomID))
                .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();
            return report;
        }

        private static ReportSummaryDto BuildBase(
            List<DAL.Entities.Room> rooms,
            List<DAL.Entities.Contract> contracts,
            List<DAL.Entities.Invoice> paid,
            DateTime now)
        {
            int occupied = rooms.Count(r => r.Status == "Occupied");
            int available = rooms.Count(r => r.Status == "Available");
            int totalRooms = Math.Max(1, rooms.Count);

            var revenueByMonth = new List<RevenueChartData>();
            for (int i = 5; i >= 0; i--)
            {
                var m = now.AddMonths(-i);
                revenueByMonth.Add(new RevenueChartData
                {
                    Month = m.Month,
                    Total = paid.Where(x => x.PaidDate != null && x.PaidDate.Value.Month == m.Month && x.PaidDate.Value.Year == m.Year)
                        .Sum(x => x.Total)
                });
            }

            return new ReportSummaryDto
            {
                TotalRevenue = paid.Sum(i => i.Total),
                MonthlyRevenue = paid.Where(i => i.PaidDate != null && i.PaidDate.Value.Month == now.Month && i.PaidDate.Value.Year == now.Year)
                    .Sum(i => i.Total),
                TotalContracts = contracts.Count,
                ActiveContracts = contracts.Count(c => c.Status == "Active"),
                OccupiedRooms = occupied,
                AvailableRooms = available,
                OccupancyRate = Math.Round(100.0 * occupied / totalRooms, 1),
                RevenueByMonth = revenueByMonth
            };
        }
    }
}
