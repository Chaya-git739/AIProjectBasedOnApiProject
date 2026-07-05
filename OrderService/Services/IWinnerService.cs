using OrderService.Models;

namespace OrderService.Services
{
    public interface IWinnerService
    {
        Task<WinnerModel> CreateWinnerAsync(int giftId, int userId);
        Task<List<WinnerModel>> GetWinnersAsync();
    }
}
