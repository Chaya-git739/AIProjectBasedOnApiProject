namespace OrderService.Services
{
    public interface IRaffleService
    {
        Task<object?> RunRaffleAsync(int giftId);
    }
}
