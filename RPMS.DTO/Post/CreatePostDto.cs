namespace RPMS.DTO.Post
{
    public class CreatePostDto
    {
        public int RoomID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal PriceSnapshot { get; set; }
        public int ExpiryMonths { get; set; }
    }
}