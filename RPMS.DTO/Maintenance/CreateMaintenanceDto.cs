namespace RPMS.DTO.Maintenance
{
    public class CreateMaintenanceDto
    {
        public int ContractID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = "";
    }
}