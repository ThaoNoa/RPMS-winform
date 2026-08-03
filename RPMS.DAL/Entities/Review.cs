using System;

namespace RPMS.DAL.Entities
{
    public class Review
    {
        public int ReviewID { get; set; }
        public int ContractID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? LandlordReply { get; set; }
        public DateTime? LandlordReplyDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Contract Contract { get; set; } = null!;
    }
}
