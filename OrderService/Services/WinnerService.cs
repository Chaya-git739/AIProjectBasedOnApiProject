using OrderService.Models;

namespace OrderService.Services
{
    public class WinnerService : IWinnerService
    {
        private readonly IWinnerRepository _winnerRepository;

        public WinnerService(IWinnerRepository winnerRepository)
        {
            _winnerRepository = winnerRepository;
        }

        public Task<WinnerModel> CreateWinnerAsync(int giftId, int userId)
        {
            var winner = new WinnerModel
            {
                GiftId = giftId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            return _winnerRepository.AddWinnerAsync(winner);
        }

        public Task<List<WinnerModel>> GetWinnersAsync() =>
            _winnerRepository.GetAllWinnersAsync();
    }
}
