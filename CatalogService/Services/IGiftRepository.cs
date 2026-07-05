using CatalogService.Models;

namespace CatalogService.Services
{
    public interface IGiftRepository
    {
        Task<List<GiftModel>> GetAllAsync();
        Task<GiftModel?> GetByIdAsync(int id);
        Task<GiftModel> AddAsync(GiftModel gift);
        Task<GiftModel> UpdateAsync(GiftModel gift);
        Task DeleteAsync(GiftModel gift);
    }
}
