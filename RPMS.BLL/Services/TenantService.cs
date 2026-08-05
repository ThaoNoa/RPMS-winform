using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Contract;
using RPMS.DTO.Invoice;
using RPMS.DTO.Maintenance;
using RPMS.DTO.Notification;
using RPMS.DTO.Post;
using RPMS.DTO.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TenantService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TenantDashboardDto> GetTenantDashboardAsync(int tenantId)
        {
            var dashboard = new TenantDashboardDto();

            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.TenantID == tenantId && c.Status == "Active", "Room");
            if (contract != null)
            {
                dashboard.CurrentContract = _mapper.Map<ContractDto>(contract);

                var invoices = await _unitOfWork.Invoices.FindAsync(
                    i => i.ContractID == contract.ContractID && i.Status == "Unpaid", "Contract.Room");
                dashboard.UnpaidInvoices = _mapper.Map<List<InvoiceDto>>(invoices);

                var maintenances = await _unitOfWork.MaintenanceRequests.FindAsync(
                    m => m.ContractID == contract.ContractID, "Contract.Room,Manager");
                dashboard.RecentMaintenances = _mapper.Map<List<MaintenanceRequestDto>>(
                    maintenances.OrderByDescending(m => m.CreatedDate).Take(5));
            }

            var appointments = await _unitOfWork.Appointments.FindAsync(
                a => a.TenantID == tenantId && a.AppointmentDate >= DateTime.Now && a.Status != "Cancelled" && a.Status != "Rejected",
                "Room");
            dashboard.UpcomingAppointments = appointments
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new AppointmentDto
                {
                    AppointmentID = a.AppointmentID,
                    RoomID = a.RoomID,
                    TenantID = a.TenantID,
                    AppointmentDate = a.AppointmentDate,
                    Note = a.Note,
                    Status = a.Status,
                    RoomNumber = a.Room?.RoomNumber ?? ""
                })
                .ToList();

            var notifs = await _unitOfWork.Notifications.FindAsync(n => n.UserID == tenantId);
            dashboard.RecentNotifications = notifs
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new NotificationDto
                {
                    NotificationID = n.NotificationID,
                    UserID = n.UserID,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate,
                    UpdatedDate = n.UpdatedDate
                })
                .ToList();

            dashboard.FavoriteCount = await _unitOfWork.Favorites.CountAsync(f => f.UserID == tenantId);
            return dashboard;
        }

        public async Task<IEnumerable<PostDto>> SearchRoomsAsync(RoomSearchFilterDto filter)
        {
            filter ??= new RoomSearchFilterDto();
            bool needAmenities =
                filter.HasAirConditioner == true ||
                filter.HasWifi == true ||
                filter.HasWashingMachine == true ||
                filter.HasParking == true ||
                filter.AllowPet == true;

            string includes = needAmenities
                ? "Room.House, Room.RoomAmenities.Amenity, Room.RoomImages, PostImages"
                : "Room.House, Room.RoomImages, PostImages";

            var posts = await _unitOfWork.Posts.FindAsync(
                p => p.Status == "Approved" && (p.ExpiryDate == null || p.ExpiryDate >= DateTime.Now),
                includes).ConfigureAwait(false);
            var query = posts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLowerInvariant();
                query = query.Where(p =>
                    p.Title.ToLowerInvariant().Contains(keyword) ||
                    (p.Room?.House?.Address ?? "").ToLowerInvariant().Contains(keyword) ||
                    (p.Room?.House?.HouseName ?? "").ToLowerInvariant().Contains(keyword));
            }

            if (filter.MinPrice.HasValue) query = query.Where(p => p.PriceSnapshot >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) query = query.Where(p => p.PriceSnapshot <= filter.MaxPrice.Value);
            if (filter.Bedrooms.HasValue && filter.Bedrooms.Value > 0)
            {
                if (filter.Bedrooms.Value >= 4)
                    query = query.Where(p => p.Room != null && p.Room.Bedroom >= 4);
                else
                    query = query.Where(p => p.Room != null && p.Room.Bedroom == filter.Bedrooms.Value);
            }

            if (filter.AreaFilter.HasValue)
            {
                query = filter.AreaFilter.Value switch
                {
                    1 => query.Where(p => p.Room != null && p.Room.Area < 25),
                    2 => query.Where(p => p.Room != null && p.Room.Area >= 25 && p.Room.Area <= 50),
                    3 => query.Where(p => p.Room != null && p.Room.Area > 50 && p.Room.Area <= 100),
                    4 => query.Where(p => p.Room != null && p.Room.Area > 100),
                    _ => query
                };
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var city = filter.City.ToLowerInvariant();
                query = query.Where(p => (p.Room?.House?.Address ?? "").ToLowerInvariant().Contains(city));
            }
            if (!string.IsNullOrWhiteSpace(filter.District))
            {
                var district = filter.District.ToLowerInvariant();
                query = query.Where(p => (p.Room?.House?.Address ?? "").ToLowerInvariant().Contains(district));
            }

            query = ApplyAmenityFilter(query, filter.HasAirConditioner, new[] { "điều hòa", "dieu hoa", "air", "ac" });
            query = ApplyAmenityFilter(query, filter.HasWifi, new[] { "wifi", "internet" });
            query = ApplyAmenityFilter(query, filter.HasWashingMachine, new[] { "máy giặt", "may giat", "washer" });
            query = ApplyAmenityFilter(query, filter.HasParking, new[] { "để xe", "de xe", "parking", "garaje", "gara" });
            query = ApplyAmenityFilter(query, filter.AllowPet, new[] { "thú cưng", "thu cung", "pet" });

            if (filter.HasFurniture == true)
                query = query.Where(p => p.Room != null && !string.IsNullOrWhiteSpace(p.Room.Furniture));

            if (!string.IsNullOrWhiteSpace(filter.RoomStatus) &&
                !string.Equals(filter.RoomStatus, "All", StringComparison.OrdinalIgnoreCase))
            {
                var st = filter.RoomStatus.Trim();
                query = query.Where(p => p.Room != null && p.Room.Status == st);
            }

            if (filter.FeaturedOnly == true)
                query = query.Where(p => p.IsFeatured);

            if (filter.MinRating.HasValue && filter.MinRating.Value > 0)
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync("Contract.Room").ConfigureAwait(false);
                var ratingByRoom = reviews
                    .GroupBy(r => r.Contract.RoomID)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Rating));
                query = query.Where(p => ratingByRoom.TryGetValue(p.RoomID, out var avg) && avg >= filter.MinRating.Value);
            }

            query = filter.SortBy switch
            {
                "PriceAsc" => query.OrderBy(p => p.PriceSnapshot),
                "PriceDesc" => query.OrderByDescending(p => p.PriceSnapshot),
                "Rating" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.ViewCount),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedDate)
            };

            return _mapper.Map<IEnumerable<PostDto>>(query.ToList());
        }

        private static IEnumerable<Post> ApplyAmenityFilter(IEnumerable<Post> query, bool? flag, string[] keywords)
        {
            if (flag != true) return query;
            return query.Where(p =>
                p.Room?.RoomAmenities != null &&
                p.Room.RoomAmenities.Any(ra =>
                    keywords.Any(k => (ra.Amenity?.AmenityName ?? "").ToLowerInvariant().Contains(k))));
        }

        public async Task<bool> SendContractRequestAsync(int tenantId, int contractId, string requestType, string details)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId && c.TenantID == tenantId,
                "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);

            int landlordId = contract.Room.House.OwnerID;
            string tenantName = contract.Tenant?.FullName ?? "Khách thuê";
            string roomNumber = contract.Room?.RoomNumber ?? "?";

            var notif = new Notification
            {
                UserID = landlordId,
                Title = $"Yêu cầu {requestType} hợp đồng {contract.ContractCode}",
                Content = $"Khách thuê {tenantName} (Phòng {roomNumber}) yêu cầu {requestType}. Chi tiết: {details}",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
