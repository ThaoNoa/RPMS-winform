using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PostDto>> GetAllActivePostsAsync()
        {
            var posts = await _unitOfWork.Posts.FindAsync(
                p => p.Status == "Approved" && (p.ExpiryDate == null || p.ExpiryDate >= DateTime.Now),
                "Room.House, PostImages");
            return _mapper.Map<IEnumerable<PostDto>>(posts.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedDate));
        }

        public async Task<IEnumerable<PostDto>> GetPendingPostsAsync()
        {
            var posts = await _unitOfWork.Posts.FindAsync(p => p.Status == "Pending", "Room.House, PostImages");
            return _mapper.Map<IEnumerable<PostDto>>(posts.OrderByDescending(p => p.CreatedDate));
        }

        public async Task<PostDetailDto> GetPostByIdAsync(int id)
        {
            var post = await _unitOfWork.Posts.FirstOrDefaultAsync(
                p => p.PostID == id,
                "Room.House, Room.RoomAmenities.Amenity, PostImages");
            if (post == null) throw new NotFoundException("Tin đăng", id);
            return _mapper.Map<PostDetailDto>(post);
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto request)
        {
            var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.RoomID == request.RoomID, "House");
            if (room == null) throw new NotFoundException("Phòng", request.RoomID);
            if (room.Status != "Available")
                throw new BadRequestException("Chỉ có thể đăng tin cho phòng đang trống.");
            var post = new Post
            {
                RoomID = request.RoomID,
                Title = request.Title,
                Description = request.Description,
                PriceSnapshot = request.PriceSnapshot > 0 ? request.PriceSnapshot : room.Price,
                Status = "Pending",
                ViewCount = 0,
                ExpiryDate = DateTime.Now.AddMonths(request.ExpiryMonths),
                IsFeatured = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PostDto>(post);
        }

        public async Task<bool> ApprovePostAsync(int postId, int approvedByAdminId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId);
            if (post == null) throw new NotFoundException("Tin đăng", postId);
            if (post.Status != "Pending")
                throw new BadRequestException("Chỉ có thể duyệt tin đang chờ duyệt.");
            post.Status = "Approved";
            post.ApprovedBy = approvedByAdminId;
            post.ApprovedDate = DateTime.Now;
            post.UpdatedDate = DateTime.Now;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectPostAsync(int postId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId);
            if (post == null) throw new NotFoundException("Tin đăng", postId);
            post.Status = "Rejected";
            post.UpdatedDate = DateTime.Now;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int postId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId);
            if (post != null)
            {
                post.ViewCount += 1;
                _unitOfWork.Posts.Update(post);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}