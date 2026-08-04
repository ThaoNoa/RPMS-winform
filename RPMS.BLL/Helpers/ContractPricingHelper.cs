using System;

namespace RPMS.BLL.Helpers
{
    /// <summary>
    /// Tính tiền điện/nước/thuê khi giá đổi giữa tháng (ngày xác nhận HĐ).
    /// Ngày trước PriceEffectiveDate dùng giá cũ; từ ngày đó dùng giá mới.
    /// </summary>
    public static class ContractPricingHelper
    {
        public static decimal WeightedUnitCost(decimal usage, decimal currentPrice, decimal? previousPrice, DateTime? priceEffectiveDate, DateTime monthStart)
        {
            int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var monthEnd = monthStart.AddMonths(1);

            if (!previousPrice.HasValue || !priceEffectiveDate.HasValue)
                return Round(usage * currentPrice);

            var eff = priceEffectiveDate.Value.Date;
            if (eff <= monthStart)
                return Round(usage * currentPrice);
            if (eff >= monthEnd)
                return Round(usage * previousPrice.Value);

            int daysOld = (eff - monthStart).Days;
            int daysNew = daysInMonth - daysOld;
            if (daysOld < 0) daysOld = 0;
            if (daysNew < 0) daysNew = 0;
            decimal avg = (previousPrice.Value * daysOld + currentPrice * daysNew) / daysInMonth;
            return Round(usage * avg);
        }

        /// <summary>Tiền thuê theo ngày ở, kết hợp đổi giá giữa tháng.</summary>
        public static decimal CalculateRent(
            decimal currentRent,
            decimal? previousRent,
            DateTime? priceEffectiveDate,
            DateTime monthStart,
            DateTime contractStart,
            DateTime contractEnd,
            DateTime? moveIn,
            DateTime? moveOut)
        {
            int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var monthEnd = monthStart.AddMonths(1);
            var occStart = MaxDate(monthStart, contractStart.Date, (moveIn ?? contractStart).Date);
            var occEndExclusive = MinDate(monthEnd, contractEnd.Date.AddDays(1),
                moveOut.HasValue ? moveOut.Value.Date.AddDays(1) : monthEnd);

            if (occEndExclusive <= occStart)
                return 0;

            if (!previousRent.HasValue || !priceEffectiveDate.HasValue ||
                priceEffectiveDate.Value.Date <= monthStart ||
                priceEffectiveDate.Value.Date >= monthEnd)
            {
                int occupied = (occEndExclusive - occStart).Days;
                return Round(currentRent * occupied / daysInMonth);
            }

            var eff = priceEffectiveDate.Value.Date;
            // Đoạn giá cũ: [occStart, min(occEnd, eff))
            var oldEnd = MinDate(occEndExclusive, eff);
            int daysOld = oldEnd > occStart ? (oldEnd - occStart).Days : 0;
            // Đoạn giá mới: [max(occStart, eff), occEnd)
            var newStart = MaxDate(occStart, eff);
            int daysNew = occEndExclusive > newStart ? (occEndExclusive - newStart).Days : 0;

            return Round(previousRent.Value * daysOld / daysInMonth + currentRent * daysNew / daysInMonth);
        }

        private static DateTime MaxDate(DateTime a, DateTime b, DateTime c)
        {
            var m = a > b ? a : b;
            return m > c ? m : c;
        }

        private static DateTime MaxDate(DateTime a, DateTime b) => a > b ? a : b;
        private static DateTime MinDate(DateTime a, DateTime b) => a < b ? a : b;
        private static DateTime MinDate(DateTime a, DateTime b, DateTime c)
        {
            var m = a < b ? a : b;
            return m < c ? m : c;
        }

        private static decimal Round(decimal v) =>
            Math.Round(v, 0, MidpointRounding.AwayFromZero);
    }
}
