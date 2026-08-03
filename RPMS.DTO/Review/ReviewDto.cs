using System;

namespace RPMS.DTO.Review
{
    public class ReviewDto
    {
        public int ReviewID { get; set; }
        public int ContractID { get; set; }
        public string ContractCode { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string TenantName { get; set; } = "";
        public string LandlordName { get; set; } = "";
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? LandlordReply { get; set; }
        public DateTime? LandlordReplyDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateReviewDto
    {
        public int ContractID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class ReplyReviewDto
    {
        public int ReviewID { get; set; }
        public string Reply { get; set; } = "";
    }
}
