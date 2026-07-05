using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public interface IGiftService
    {
        Task<List<GiftDto>> GetAllAsync();
        Task<GiftDto> AddAsync(GiftDto gift);
        Task<GiftDto?> UpdateAsync(int id, GiftDto gift);
        Task<bool> DeleteAsync(int id);
    }
}
