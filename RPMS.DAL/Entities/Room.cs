using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Room
    {
        public Room()
        {
            RoomImages = new HashSet<RoomImage>();
            RoomAmenities = new HashSet<RoomAmenity>();
            Posts = new HashSet<Post>();
            Favorites = new HashSet<Favorite>();
            Appointments = new HashSet<Appointment>();
            Contracts = new HashSet<Contract>();
        }

        public int RoomID { get; set; }
        public int HouseID { get; set; }
        public string RoomNumber { get; set; } = "";
        public int? Floor { get; set; }
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Bedroom { get; set; }
        public int Bathroom { get; set; }
        public string? Furniture { get; set; }
        public string Status { get; set; } = "";
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual House House { get; set; } = null!;
        public virtual ICollection<RoomImage> RoomImages { get; set; }
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
    }
}