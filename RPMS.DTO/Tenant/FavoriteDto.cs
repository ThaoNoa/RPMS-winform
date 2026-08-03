using System;

namespace RPMS.DTO.Tenant
{
    public class FavoriteDto
    {
        public int FavoriteID { get; set; }
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = "";
        public string HouseName { get; set; } = "";
        public string HouseAddress { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }
}
