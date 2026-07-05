using OrderService.Models;

namespace OrderService.Services
{
    public interface IWinnerRepository
    {
        Task<WinnerModel> AddWinnerAsync(WinnerModel winner);
        Task<List<WinnerModel>> GetAllWinnersAsync();
        Task<bool> IsGiftAlreadyWonAsync(int giftId);
    }
}
