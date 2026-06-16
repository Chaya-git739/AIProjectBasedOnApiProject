using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication2.Models.DTO;

namespace WebApplication2.DAL
{
    public interface IGiftDal
    {
        Task<List<GiftDTO>> GetByFilter(string? name, string? donorName, int? minPurchasers);
        Task<List<GiftDTO>> GetGiftsSortedByPrice();
        Task<List<GiftDTO>> GetMostPurchasedGifts();
        Task<List<GiftDTO>> GetAll();
        Task Add(GiftDTO gift);
        Task Update(GiftDTO gift);
        Task Delete(int id);
        Task<decimal> GetTotalSalesAsync();
        Task<List<GiftSalesDto>> GetSalesPerGiftAsync();
        Task<List<GiftWinnerDto>> GetGiftsWithWinnersAsync();
    }
}