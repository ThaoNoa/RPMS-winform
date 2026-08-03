using System;

namespace RPMS.DTO.Post
{
    public class PostDto
    {
        public int PostID { get; set; }
        public int RoomID { get; set; }
        public string Title { get; set; } = "";
        public decimal PriceSnapshot { get; set; }
        public string Status { get; set; } = "";
        public int ViewCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsFeatured { get; set; }
        public string MainImage { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string HouseAddress { get; set; } = "";
    }
}