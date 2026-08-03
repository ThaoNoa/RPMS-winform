using System.Collections.Generic;

namespace RPMS.DTO.Post
{
    public class PostDetailDto : PostDto
    {
        public string Description { get; set; } = "";
        public decimal Area { get; set; }
        public string Furniture { get; set; } = "";
        public List<string> Images { get; set; } = new();
        public List<string> Amenities { get; set; } = new();
    }
}