namespace RPMS.DTO.Room
{
    public class UpdateRoomDto
    {
        public string RoomNumber { get; set; } = "";
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Bedroom { get; set; }
        public int Bathroom { get; set; }
        public string Furniture { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
    }
}