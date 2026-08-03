using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class HouseRepository : GenericRepository<House>, IHouseRepository
    {
        public HouseRepository(RPMSContext context) : base(context)
        {
        }
    }
}