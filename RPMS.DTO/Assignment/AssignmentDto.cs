using System;

namespace RPMS.DTO.Assignment
{
    public class AssignmentDto
    {
        public int AssignmentID { get; set; }
        public int HouseID { get; set; }
        public string HouseName { get; set; } = "";
        public string HouseAddress { get; set; } = "";
        public int ManagerID { get; set; }
        public string ManagerName { get; set; } = "";
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class CreateAssignmentDto
    {
        public int HouseID { get; set; }
        public int ManagerID { get; set; }
    }
}
