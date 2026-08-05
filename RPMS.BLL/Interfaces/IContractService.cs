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
        /// <summary>Tạo HĐ nháp cho mọi phòng của nhà chưa có HĐ Active/Draft.</summary>
        Task<BulkCreateDraftContractsResultDto> CreateDraftContractsForHouseAsync(BulkCreateDraftContractsDto request, int landlordId);
        Task<ContractDto> AssignTenantAsync(AssignTenantDto request, int actorUserId);
        /// <summary>Khách đồng ý thuê — PendingConfirm → Active, phòng Occupied.</summary>
        Task<bool> AcceptRentalOfferAsync(int contractId, int tenantId);
        /// <summary>Khách từ chối thuê — về Draft, bỏ TenantID.</summary>
        Task<bool> RejectRentalOfferAsync(int contractId, int tenantId);
        Task<ContractDto> UpdateContractAsync(UpdateContractDto request, int landlordId);
        Task<bool> ConfirmContractEditAsync(int contractId, int tenantId);
        Task<bool> RejectContractEditAsync(int contractId, int tenantId);
        Task<bool> CancelPendingContractEditAsync(int contractId, int landlordId);
        /// <summary>Xin hủy thuê Active (Tenant hoặc Landlord) — chờ bên kia duyệt.</summary>
        Task<bool> RequestCancelAsync(int contractId, int actorUserId, string reason);
        /// <summary>Bên kia duyệt yêu cầu hủy → Terminated.</summary>
        Task<bool> ApproveCancelRequestAsync(int contractId, int actorUserId);
        /// <summary>Bên kia từ chối yêu cầu hủy — HĐ vẫn Active.</summary>
        Task<bool> RejectCancelRequestAsync(int contractId, int actorUserId, string? note = null);
        Task<bool> TerminateContractAsync(int contractId);
        Task<bool> TerminateContractAsync(int contractId, int actorUserId, string? reason = null);
        Task<bool> ExtendContractAsync(int contractId, DateTime newEndDate, int actorUserId);
    }
}