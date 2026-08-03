using RPMS.DTO.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IContractService
    {
        Task<IEnumerable<ContractDto>> GetAllContractsAsync();
        Task<IEnumerable<ContractDto>> GetContractsByTenantAsync(int tenantId);
        Task<IEnumerable<ContractDto>> GetContractsByLandlordAsync(int landlordId);
        Task<IEnumerable<ContractDto>> GetContractsByManagerAsync(int managerId);
        Task<ContractDetailDto> GetContractByIdAsync(int id);
        Task<ContractDto> CreateContractAsync(CreateContractDto request, int createdById);
        Task<bool> TerminateContractAsync(int contractId);
        Task<bool> ExtendContractAsync(int contractId, DateTime newEndDate, int actorUserId);
    }
}