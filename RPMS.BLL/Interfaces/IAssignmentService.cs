using RPMS.DTO.Assignment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetAllAsync();
        Task<IEnumerable<AssignmentDto>> GetByLandlordAsync(int landlordId);
        Task<IEnumerable<AssignmentDto>> GetByManagerAsync(int managerId);
        Task<AssignmentDto> CreateAsync(CreateAssignmentDto request);
        Task<bool> DeactivateAsync(int assignmentId);
    }
}
