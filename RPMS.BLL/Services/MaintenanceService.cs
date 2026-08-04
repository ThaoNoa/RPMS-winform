using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaintenanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaintenanceRequestDto>> GetRequestsByHouseAsync(int houseId)
        {
            var requests = await _unitOfWork.MaintenanceRequests.FindAsync(
                m => m.Contract.Room.HouseID == houseId,
                "Contract.Room, Manager");
            return _mapper.Map<IEnumerable<MaintenanceRequestDto>>(requests);
        }

        public async Task<IEnumerable<MaintenanceRequestDto>> GetRequestsByTenantAsync(int tenantId)
        {
            var requests = await _unitOfWork.MaintenanceRequests.FindAsync(
                m => m.Contract.TenantID == tenantId,
                "Contract.Room, Manager");
            return _mapper.Map<IEnumerable<MaintenanceRequestDto>>(requests);
        }

        public async Task<MaintenanceRequestDto> CreateRequestAsync(CreateMaintenanceDto request)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractID);
            if (contract == null) throw new NotFoundException("Hợp đồng", request.ContractID);

            var maintenance = new MaintenanceRequest
            {
                ContractID = request.ContractID,
                Title = request.Title,
                Description = request.Description,
                Image = request.ImagePath,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.MaintenanceRequests.AddAsync(maintenance);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.MaintenanceRequests.FirstOrDefaultAsync(m => m.RequestID == maintenance.RequestID, "Contract.Room, Manager");
            return _mapper.Map<MaintenanceRequestDto>(result);
        }

        public async Task<bool> UpdateRequestStatusAsync(int requestId, string status, int managerId)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
            if (request == null) throw new NotFoundException("Yêu cầu bảo trì", requestId);
            request.Status = status;
            request.UpdatedDate = DateTime.Now;
            if (status == "Processing")
                request.AssignedManager = managerId;
            else if (status == "Completed")
                request.CompletedDate = DateTime.Now;
            _unitOfWork.MaintenanceRequests.Update(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Thêm vào class MaintenanceService:
        public async Task<IEnumerable<MaintenanceRequestDto>> GetRequestsForManagerAsync(int managerId)
        {
            var assignments = await _unitOfWork.Assignments.FindAsync(a => a.ManagerID == managerId && a.Status == "Active");
            var houseIds = assignments.Select(a => a.HouseID).Distinct().ToList();
            if (houseIds.Count == 0)
                return Array.Empty<MaintenanceRequestDto>();

            var roomIds = (await _unitOfWork.Rooms.FindAsync(r => houseIds.Contains(r.HouseID)))
                .Select(r => r.RoomID)
                .ToList();
            if (roomIds.Count == 0)
                return Array.Empty<MaintenanceRequestDto>();

            var contractIds = (await _unitOfWork.Contracts.FindAsync(c => roomIds.Contains(c.RoomID)))
                .Select(c => c.ContractID)
                .ToList();
            if (contractIds.Count == 0)
                return Array.Empty<MaintenanceRequestDto>();

            var requests = await _unitOfWork.MaintenanceRequests.FindAsync(
                m => contractIds.Contains(m.ContractID),
                "Contract.Room.House,Contract.Tenant,Manager");

            return _mapper.Map<IEnumerable<MaintenanceRequestDto>>(requests.OrderByDescending(r => r.CreatedDate));
        }

        public async Task<MaintenanceRequestDto> GetRequestByIdAsync(int requestId)
        {
            var request = await _unitOfWork.MaintenanceRequests.FirstOrDefaultAsync(
                m => m.RequestID == requestId,
                "Contract.Room.House,Contract.Tenant,Manager");
            if (request == null) throw new NotFoundException("Yêu cầu bảo trì", requestId);
            return _mapper.Map<MaintenanceRequestDto>(request);
        }

        public async Task<bool> DeleteRequestAsync(int requestId)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
            if (request != null)
            {
                _unitOfWork.MaintenanceRequests.Remove(request);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SendMaintenanceNotificationAsync(int requestId, string message)
        {
            var request = await _unitOfWork.MaintenanceRequests.FirstOrDefaultAsync(
                m => m.RequestID == requestId, "Contract.Room");
            if (request != null && request.Contract?.TenantID is int tenantId)
            {
                string roomLabel = request.Contract?.Room?.RoomNumber ?? request.ContractID.ToString();
                var notif = new Notification
                {
                    UserID = tenantId,
                    Title = "Phản hồi Yêu cầu Bảo trì",
                    Content = $"Phòng {roomLabel}: {message}",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Notifications.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}