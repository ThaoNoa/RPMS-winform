using System;

namespace RPMS.DAL.Entities
{
    public class Assignment
    {
        public int AssignmentID { get; set; }
        public int HouseID { get; set; }
        public int ManagerID { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual House House { get; set; } = null!;
        public virtual User Manager { get; set; } = null!;
    }
}