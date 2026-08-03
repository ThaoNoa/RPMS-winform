using System;

namespace RPMS.BLL.Helpers
{
    public sealed class RentProrationResult
    {
        public decimal FullMonthlyRent { get; init; }
        public decimal ProratedRent { get; init; }
        public int DaysInMonth { get; init; }
        public int OccupiedDays { get; init; }
        public DateTime OccupancyFrom { get; init; }
        public DateTime OccupancyTo { get; init; }
        public bool IsProrated => OccupiedDays < DaysInMonth;
        public string Note => IsProrated
            ? $"Tiền nhà prorate: {FullMonthlyRent:N0} ÷ {DaysInMonth} ngày × {OccupiedDays} ngày ở ({OccupancyFrom:dd/MM}–{OccupancyTo:dd/MM})"
            : $"Tiền nhà đủ tháng ({DaysInMonth} ngày)";
    }

    /// <summary>
    /// Tính tiền nhà theo ngày thực ở trong tháng.
    /// Công thức: MonthlyRent / số ngày trong tháng × số ngày ở
    /// (trừ những ngày chưa nhận phòng / đã trả phòng).
    /// </summary>
    public static class RentProrationHelper
    {
        public static RentProrationResult Calculate(
            decimal monthlyRent,
            DateTime billingMonth,
            DateTime contractStart,
            DateTime contractEnd,
            DateTime? moveInDate,
            DateTime? moveOutDate)
        {
            var monthStart = new DateTime(billingMonth.Year, billingMonth.Month, 1);
            var monthLast = monthStart.AddMonths(1).AddDays(-1);
            int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            // Khoảng thời gian được phép ở theo hợp đồng
            var stayStart = (moveInDate ?? contractStart).Date;
            var stayEnd = (moveOutDate ?? contractEnd).Date;
            if (stayEnd < stayStart)
                stayEnd = stayStart;

            // Giao với tháng thanh toán
            var from = stayStart > monthStart ? stayStart : monthStart;
            var to = stayEnd < monthLast ? stayEnd : monthLast;

            int occupiedDays = 0;
            if (to >= from)
                occupiedDays = (to - from).Days + 1;

            if (occupiedDays > daysInMonth)
                occupiedDays = daysInMonth;

            decimal prorated = occupiedDays <= 0
                ? 0
                : Math.Round(monthlyRent * occupiedDays / daysInMonth, 0, MidpointRounding.AwayFromZero);

            return new RentProrationResult
            {
                FullMonthlyRent = monthlyRent,
                ProratedRent = prorated,
                DaysInMonth = daysInMonth,
                OccupiedDays = occupiedDays,
                OccupancyFrom = occupiedDays > 0 ? from : monthStart,
                OccupancyTo = occupiedDays > 0 ? to : monthStart
            };
        }
    }
}
