using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class MeterReadingRepository : GenericRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(RPMSContext context) : base(context)
        {
        }
    }
}