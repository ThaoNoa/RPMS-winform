using System;
using System.Collections.Generic;

namespace RPMS.DTO.Calendar
{
    public class CalendarEventDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = ""; // Appointment | Contract | Maintenance | Invoice
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Status { get; set; } = "";
        public ColorHint ColorHint { get; set; }
    }

    public enum ColorHint
    {
        Primary,
        Success,
        Warning,
        Danger
    }

    public class CalendarDayDto
    {
        public DateTime Date { get; set; }
        public List<CalendarEventDto> Events { get; set; } = new();
    }
}
