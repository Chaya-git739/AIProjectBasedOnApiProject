namespace OrderService.Services
{
    public class WinnerService : IWinnerService
    {
        private static readonly List<object> Winners = new();

        public Task<object> CreateWinnerAsync(int giftId, int userId)
        {
            var winner = new
            {
                giftId,
                userId,
                message = "זוכה נשמר"
            };

            Winners.Add(winner);
            return Task.FromResult<object>(winner);
        }

        public Task<List<object>> GetWinnersAsync()
        {
            return Task.FromResult(Winners.ToList());
        }
    }
}
