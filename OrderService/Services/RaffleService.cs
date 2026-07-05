namespace OrderService.Services
{
    public class RaffleService : IRaffleService
    {
        private readonly IOrderRepository _orderRepository;

        public RaffleService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<object?> RunRaffleAsync(int giftId)
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var eligibleOrders = orders
                .Where(o => !o.IsDraft)
                .ToList();

            if (eligibleOrders.Count == 0)
            {
                return null;
            }

            var random = new Random();
            var winner = eligibleOrders[random.Next(eligibleOrders.Count)];

            return new
            {
                giftId,
                userId = winner.UserId,
                message = "זכה בהגרלה"
            };
        }
    }
}
