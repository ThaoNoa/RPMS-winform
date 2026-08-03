using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(RPMSContext context) : base(context)
        {
        }
    }
}