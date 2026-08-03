using System;

namespace RPMS.DAL.Entities
{
    public class PostImage
    {
        public int PostImageID { get; set; }
        public int PostID { get; set; }
        public string ImagePath { get; set; } = "";
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Post Post { get; set; } = null!;
    }
}