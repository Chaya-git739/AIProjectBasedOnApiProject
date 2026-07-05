namespace OrderService.Services
{
    public interface IWinnerService
    {
        Task<object> CreateWinnerAsync(int giftId, int userId);
        Task<List<object>> GetWinnersAsync();
    }
}
