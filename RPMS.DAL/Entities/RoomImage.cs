using System;

namespace RPMS.DAL.Entities
{
    public class RoomImage
    {
        public int ImageID { get; set; }
        public int RoomID { get; set; }
        public string ImagePath { get; set; } = "";
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}