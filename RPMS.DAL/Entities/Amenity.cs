using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Amenity
    {
        public Amenity()
        {
            RoomAmenities = new HashSet<RoomAmenity>();
        }

        public int AmenityID { get; set; }
        public string AmenityName { get; set; } = "";

        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; }
    }
}