namespace RPMS.DTO.House
{
    public class HouseDto
    {
        public int HouseID { get; set; }
        public int OwnerID { get; set; }
        public string OwnerName { get; set; } = "";
        public string HouseName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public int TotalRooms { get; set; }
    }
}