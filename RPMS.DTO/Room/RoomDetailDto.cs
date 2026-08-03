using System.Collections.Generic;
using RPMS.DTO.Amenity;

namespace RPMS.DTO.Room
{
    public class RoomDetailDto : RoomDto
    {
        public int Capacity { get; set; }
        public int Bedroom { get; set; }
        public int Bathroom { get; set; }
        public string Furniture { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Images { get; set; } = new();
        public List<AmenityDto> Amenities { get; set; } = new();
    }
}