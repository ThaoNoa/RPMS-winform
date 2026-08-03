using System;
using System.Collections.Generic;

namespace RPMS.DTO.Post
{
    public class RoomSearchFilterDto
    {
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? AreaFilter { get; set; }
        public int? Bedrooms { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public bool? HasAirConditioner { get; set; }
        public bool? HasWifi { get; set; }
        public bool? HasWashingMachine { get; set; }
        public bool? HasFurniture { get; set; }
        public bool? AllowPet { get; set; }
        public bool? HasParking { get; set; }
        public int? MinRating { get; set; }
        public string SortBy { get; set; } = "Newest"; // Newest | PriceAsc | PriceDesc | Rating
    }
}
