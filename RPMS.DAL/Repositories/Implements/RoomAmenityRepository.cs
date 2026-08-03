using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class RoomAmenityRepository : GenericRepository<RoomAmenity>, IRoomAmenityRepository
    {
        public RoomAmenityRepository(RPMSContext context) : base(context)
        {
        }
    }
}