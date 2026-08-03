using RPMS.DTO.Calendar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface ICalendarService
    {
        Task<IEnumerable<CalendarEventDto>> GetEventsAsync(int userId, int roleId, DateTime from, DateTime to);
    }
}
