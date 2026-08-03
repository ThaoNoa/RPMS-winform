namespace RPMS.DAL.Entities
{
    public class RoomAmenity
    {
        public int RoomAmenityID { get; set; }
        public int RoomID { get; set; }
        public int AmenityID { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual Amenity Amenity { get; set; } = null!;
    }
}