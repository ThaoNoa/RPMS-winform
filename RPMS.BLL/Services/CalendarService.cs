using RPMS.BLL.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CalendarService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CalendarEventDto>> GetEventsAsync(int userId, int roleId, DateTime from, DateTime to)
        {
            var events = new List<CalendarEventDto>();
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1).AddTicks(-1);

            if (roleId == 1)
            {
                await AddAllAsync(events, fromDate, toDate);
            }
            else if (roleId == 2)
            {
                var houseIds = (await _unitOfWork.Houses.FindAsync(h => h.OwnerID == userId)).Select(h => h.HouseID).ToList();
                await AddForHousesAsync(events, houseIds, fromDate, toDate);
            }
            else if (roleId == 3)
            {
                var appointments = await _unitOfWork.Appointments.FindAsync(
                    a => a.TenantID == userId && a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate, "Room");
                foreach (var a in appointments)
                {
                    events.Add(new CalendarEventDto
                    {
                        Date = a.AppointmentDate,
                        Type = "Appointment",
                        Title = $"Xem phòng {a.Room?.RoomNumber}",
                        Detail = a.Note ?? "",
                        Status = a.Status,
                        ColorHint = ColorHint.Primary
                    });
                }

                var contracts = await _unitOfWork.Contracts.FindAsync(
                    c => c.TenantID == userId &&
                         ((c.StartDate >= fromDate && c.StartDate <= toDate) ||
                          (c.EndDate >= fromDate && c.EndDate <= toDate)),
                    "Room");
                foreach (var c in contracts)
                {
                    events.Add(new CalendarEventDto
                    {
                        Date = c.EndDate,
                        Type = "Contract",
                        Title = $"HĐ {c.ContractCode} hết hạn",
                        Detail = $"Phòng {c.Room?.RoomNumber}",
                        Status = c.Status,
                        ColorHint = ColorHint.Warning
                    });
                }

                var invoices = await _unitOfWork.Invoices.FindAsync(
                    i => i.Contract.TenantID == userId && i.DueDate >= fromDate && i.DueDate <= toDate,
                    "Contract.Room");
                foreach (var i in invoices)
                {
                    events.Add(new CalendarEventDto
                    {
                        Date = i.DueDate,
                        Type = "Invoice",
                        Title = $"Hóa đơn {i.InvoiceCode}",
                        Detail = $"{i.Total:N0} đ - {i.Status}",
                        Status = i.Status,
                        ColorHint = i.Status == "Paid" ? ColorHint.Success : ColorHint.Danger
                    });
                }

                var maints = await _unitOfWork.MaintenanceRequests.FindAsync(
                    m => m.Contract.TenantID == userId && m.CreatedDate >= fromDate && m.CreatedDate <= toDate,
                    "Contract.Room");
                foreach (var m in maints)
                {
                    events.Add(new CalendarEventDto
                    {
                        Date = m.CreatedDate,
                        Type = "Maintenance",
                        Title = m.Title,
                        Detail = $"Phòng {m.Contract?.Room?.RoomNumber} - {m.Status}",
                        Status = m.Status,
                        ColorHint = ColorHint.Warning
                    });
                }
            }
            else if (roleId == 4)
            {
                var houseIds = (await _unitOfWork.Assignments.FindAsync(a => a.ManagerID == userId && a.Status == "Active"))
                    .Select(a => a.HouseID).ToList();
                await AddForHousesAsync(events, houseIds, fromDate, toDate);
            }

            return events.OrderBy(e => e.Date).ThenBy(e => e.Type).ToList();
        }

        private async Task AddAllAsync(List<CalendarEventDto> events, DateTime from, DateTime to)
        {
            var houseIds = (await _unitOfWork.Houses.GetAllAsync()).Select(h => h.HouseID).ToList();
            await AddForHousesAsync(events, houseIds, from, to);
        }

        private async Task AddForHousesAsync(List<CalendarEventDto> events, List<int> houseIds, DateTime from, DateTime to)
        {
            if (houseIds.Count == 0) return;

            var appointments = await _unitOfWork.Appointments.FindAsync(
                a => houseIds.Contains(a.Room.HouseID) && a.AppointmentDate >= from && a.AppointmentDate <= to,
                "Room");
            foreach (var a in appointments)
            {
                events.Add(new CalendarEventDto
                {
                    Date = a.AppointmentDate,
                    Type = "Appointment",
                    Title = $"Lịch hẹn phòng {a.Room?.RoomNumber}",
                    Detail = a.Note ?? "",
                    Status = a.Status,
                    ColorHint = ColorHint.Primary
                });
            }

            var contracts = await _unitOfWork.Contracts.FindAsync(
                c => houseIds.Contains(c.Room.HouseID) && c.EndDate >= from && c.EndDate <= to,
                "Room, Tenant");
            foreach (var c in contracts)
            {
                events.Add(new CalendarEventDto
                {
                    Date = c.EndDate,
                    Type = "Contract",
                    Title = $"HĐ {c.ContractCode}",
                    Detail = $"{c.Tenant?.FullName} - Phòng {c.Room?.RoomNumber}",
                    Status = c.Status,
                    ColorHint = ColorHint.Warning
                });
            }

            var invoices = await _unitOfWork.Invoices.FindAsync(
                i => houseIds.Contains(i.Contract.Room.HouseID) && i.DueDate >= from && i.DueDate <= to,
                "Contract.Room");
            foreach (var i in invoices)
            {
                events.Add(new CalendarEventDto
                {
                    Date = i.DueDate,
                    Type = "Invoice",
                    Title = $"Hóa đơn {i.InvoiceCode}",
                    Detail = $"{i.Total:N0} đ - {i.Status}",
                    Status = i.Status,
                    ColorHint = i.Status == "Paid" ? ColorHint.Success : ColorHint.Danger
                });
            }

            var maints = await _unitOfWork.MaintenanceRequests.FindAsync(
                m => houseIds.Contains(m.Contract.Room.HouseID) && m.CreatedDate >= from && m.CreatedDate <= to,
                "Contract.Room");
            foreach (var m in maints)
            {
                events.Add(new CalendarEventDto
                {
                    Date = m.CreatedDate,
                    Type = "Maintenance",
                    Title = m.Title,
                    Detail = $"Phòng {m.Contract?.Room?.RoomNumber}",
                    Status = m.Status,
                    ColorHint = ColorHint.Warning
                });
            }
        }
    }
}
