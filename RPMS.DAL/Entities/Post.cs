using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Post
    {
        public Post()
        {
            PostImages = new HashSet<PostImage>();
        }

        public int PostID { get; set; }
        public int RoomID { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public decimal PriceSnapshot { get; set; }
        public string Status { get; set; } = "";
        public int ViewCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual ICollection<PostImage> PostImages { get; set; }
    }
}