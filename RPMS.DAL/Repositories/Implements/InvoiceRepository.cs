using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(RPMSContext context) : base(context)
        {
        }
    }
}