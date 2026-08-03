using RPMS.DTO.Post;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetAllActivePostsAsync();
        Task<IEnumerable<PostDto>> GetPendingPostsAsync();
        Task<PostDetailDto> GetPostByIdAsync(int id);
        Task<PostDto> CreatePostAsync(CreatePostDto request);
        Task<bool> ApprovePostAsync(int postId, int approvedByAdminId);
        Task<bool> RejectPostAsync(int postId);
        Task<bool> IncrementViewCountAsync(int postId);
    }
}