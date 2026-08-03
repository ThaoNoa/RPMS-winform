using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReviewDto> CreateReviewAsync(int tenantId, CreateReviewDto request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new BadRequestException("Đánh giá phải từ 1 đến 5 sao.");

            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == request.ContractID && c.TenantID == tenantId,
                "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", request.ContractID);
            if (contract.Status != "Terminated" && contract.Status != "Expired")
                throw new BadRequestException("Chỉ được đánh giá sau khi hợp đồng kết thúc hoặc hết hạn.");

            var existing = await _unitOfWork.Reviews.FirstOrDefaultAsync(r => r.ContractID == request.ContractID);
            if (existing != null)
                throw new BadRequestException("Hợp đồng này đã được đánh giá.");

            var review = new Review
            {
                ContractID = request.ContractID,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.Reviews.AddAsync(review);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = contract.Room.House.OwnerID,
                Title = "Đánh giá mới",
                Content = $"Khách thuê {contract.Tenant?.FullName} đã đánh giá {request.Rating}/5 cho phòng {contract.Room.RoomNumber}.",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            var saved = await _unitOfWork.Reviews.FirstOrDefaultAsync(r => r.ReviewID == review.ReviewID, "Contract.Room.House.Owner, Contract.Tenant");
            return Map(saved!);
        }

        public async Task<bool> ReplyAsync(int landlordId, ReplyReviewDto request)
        {
            var review = await _unitOfWork.Reviews.FirstOrDefaultAsync(
                r => r.ReviewID == request.ReviewID,
                "Contract.Room.House, Contract.Tenant");
            if (review == null) throw new NotFoundException("Đánh giá", request.ReviewID);
            if (review.Contract.Room.House.OwnerID != landlordId)
                throw new BadRequestException("Bạn không có quyền phản hồi đánh giá này.");
            if (string.IsNullOrWhiteSpace(request.Reply))
                throw new BadRequestException("Nội dung phản hồi không được trống.");

            review.LandlordReply = request.Reply.Trim();
            review.LandlordReplyDate = DateTime.Now;
            review.UpdatedDate = DateTime.Now;
            _unitOfWork.Reviews.Update(review);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = review.Contract.TenantID,
                Title = "Chủ nhà phản hồi đánh giá",
                Content = review.LandlordReply,
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReviewDto>> GetByLandlordAsync(int landlordId)
        {
            var reviews = await _unitOfWork.Reviews.FindAsync(
                r => r.Contract.Room.House.OwnerID == landlordId,
                "Contract.Room.House.Owner, Contract.Tenant");
            return reviews.OrderByDescending(r => r.CreatedDate).Select(Map).ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetByTenantAsync(int tenantId)
        {
            var reviews = await _unitOfWork.Reviews.FindAsync(
                r => r.Contract.TenantID == tenantId,
                "Contract.Room.House.Owner, Contract.Tenant");
            return reviews.OrderByDescending(r => r.CreatedDate).Select(Map).ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync("Contract.Room.House.Owner, Contract.Tenant");
            return reviews.OrderByDescending(r => r.CreatedDate).Select(Map).ToList();
        }

        public async Task<double> GetAverageRatingForHouseAsync(int houseId)
        {
            var reviews = await _unitOfWork.Reviews.FindAsync(r => r.Contract.Room.HouseID == houseId, "Contract.Room");
            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.Rating);
        }

        private static ReviewDto Map(Review r) => new()
        {
            ReviewID = r.ReviewID,
            ContractID = r.ContractID,
            ContractCode = r.Contract?.ContractCode ?? "",
            RoomNumber = r.Contract?.Room?.RoomNumber ?? "",
            TenantName = r.Contract?.Tenant?.FullName ?? "",
            LandlordName = r.Contract?.Room?.House?.Owner?.FullName ?? "",
            Rating = r.Rating,
            Comment = r.Comment,
            LandlordReply = r.LandlordReply,
            LandlordReplyDate = r.LandlordReplyDate,
            CreatedDate = r.CreatedDate
        };
    }
}
