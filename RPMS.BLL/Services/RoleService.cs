using AutoMapper;
using RPMS.BLL.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Role;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }
    }
}