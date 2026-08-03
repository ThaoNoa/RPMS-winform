using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class MaintenanceRequestRepository : GenericRepository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        public MaintenanceRequestRepository(RPMSContext context) : base(context)
        {
        }
    }
}