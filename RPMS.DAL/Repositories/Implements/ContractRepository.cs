using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(RPMSContext context) : base(context)
        {
        }
    }
}