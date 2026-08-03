using System;

namespace RPMS.DAL.Entities
{
    public class Favorite
    {
        public int FavoriteID { get; set; }
        public int UserID { get; set; }
        public int RoomID { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}