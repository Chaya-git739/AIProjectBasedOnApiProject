using OrderService.Models.DTO;

namespace OrderService.Services
{
    public interface ICatalogServiceClient
    {
        Task<CatalogGiftDto?> GetGiftByIdAsync(int giftId);
    }
}
