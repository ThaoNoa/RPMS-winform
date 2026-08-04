using System;

namespace RPMS.DTO.Contract
{
    /// <summary>Tạo hợp đồng nháp hàng loạt cho phòng chưa có HĐ Active/Draft.</summary>
    public class BulkCreateDraftContractsDto
    {
        public int HouseID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Deposit { get; set; }
        /// <summary>Nếu &gt; 0 dùng chung; nếu ≤ 0 lấy giá từng phòng.</summary>
        public decimal MonthlyRent { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
    }

    public class BulkCreateDraftContractsResultDto
    {
        public int CreatedCount { get; set; }
        public int SkippedCount { get; set; }
        public string Message { get; set; } = "";
    }
}
