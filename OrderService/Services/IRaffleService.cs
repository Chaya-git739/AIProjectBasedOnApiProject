namespace OrderService.Services
{
    public interface IRaffleService
    {
        Task<RaffleWinnerResult?> RunRaffleAsync(int giftId);
    }

    public class RaffleWinnerResult
    {
        public int GiftId { get; set; }
        public int UserId { get; set; }
    }
}
