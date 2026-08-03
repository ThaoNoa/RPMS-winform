using RPMS.DTO.Review;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(int tenantId, CreateReviewDto request);
        Task<bool> ReplyAsync(int landlordId, ReplyReviewDto request);
        Task<IEnumerable<ReviewDto>> GetByLandlordAsync(int landlordId);
        Task<IEnumerable<ReviewDto>> GetByTenantAsync(int tenantId);
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<double> GetAverageRatingForHouseAsync(int houseId);
    }
}
