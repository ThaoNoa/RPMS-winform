using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class House
    {
        public House()
        {
            Rooms = new HashSet<Room>();
            Assignments = new HashSet<Assignment>();
        }

        public int HouseID { get; set; }
        public int OwnerID { get; set; }
        public string HouseName { get; set; } = "";
        public string Address { get; set; } = "";
        public string? Description { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}