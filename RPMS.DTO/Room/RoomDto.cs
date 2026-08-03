namespace RPMS.DTO.Room
{
    public class RoomDto
    {
        public int RoomID { get; set; }
        public int HouseID { get; set; }
        public string HouseName { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "";
    }
}