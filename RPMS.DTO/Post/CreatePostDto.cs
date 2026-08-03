namespace RPMS.DTO.Post
{
    public class CreatePostDto
    {
        public int RoomID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal PriceSnapshot { get; set; }
        public int ExpiryMonths { get; set; } = 1;
        /// <summary>Đường dẫn ảnh (đã copy vào /uploads/posts hoặc path gốc để service xử lý).</summary>
        public System.Collections.Generic.List<string> ImagePaths { get; set; } = new();
    }
}