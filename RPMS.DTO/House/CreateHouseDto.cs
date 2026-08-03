namespace RPMS.DTO.House
{
    public class CreateHouseDto
    {
        public int OwnerID { get; set; }
        public string HouseName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Description { get; set; } = "";
    }
}