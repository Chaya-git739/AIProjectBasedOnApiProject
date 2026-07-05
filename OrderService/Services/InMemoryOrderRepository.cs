using OrderService.Models;

namespace OrderService.Services
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private static readonly List<OrderModel> Orders = new();
        private static int NextOrderId = 1;

        public Task<int> AddOrderAsync(OrderModel order)
        {
            order.Id = NextOrderId++;
            Orders.Add(order);
            return Task.FromResult(order.Id);
        }

        public Task<List<OrderModel>> GetAllOrdersAsync()
        {
            return Task.FromResult(Orders.ToList());
        }

        public Task<List<OrderModel>> GetUserOrdersAsync(int userId)
        {
            return Task.FromResult(Orders.Where(o => o.UserId == userId).ToList());
        }

        public Task<OrderModel?> GetOrderByIdAsync(int orderId)
        {
            return Task.FromResult(Orders.FirstOrDefault(o => o.Id == orderId));
        }

        public Task<bool> ConfirmOrderAsync(int orderId)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return Task.FromResult(false);
            }

            order.IsDraft = false;
            return Task.FromResult(true);
        }

        public Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            if (giftId <= 0)
            {
                throw new BusinessException("GiftId חייב להיות גדול מאפס");
            }

            if (quantity <= 0)
            {
                throw new BusinessException("Quantity חייב להיות גדול מאפס");
            }

            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            order.OrderItems.Add(new OrderTicketModel { GiftId = giftId, Quantity = quantity });
            return Task.CompletedTask;
        }

        public Task<bool> RemoveItemAsync(int orderId, int giftId)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return Task.FromResult(false);

            var item = order.OrderItems.FirstOrDefault(i => i.GiftId == giftId);
            if (item == null) return Task.FromResult(false);

            order.OrderItems.Remove(item);
            return Task.FromResult(true);
        }

        public Task<List<(int UserId, int Quantity)>> GetRafflePoolAsync(int giftId)
        {
            var pool = Orders
                .Where(o => !o.IsDraft && o.OrderItems.Any(i => i.GiftId == giftId))
                .SelectMany(o => o.OrderItems
                    .Where(i => i.GiftId == giftId)
                    .Select(i => new { o.UserId, i.Quantity }))
                .GroupBy(x => x.UserId)
                .Select(g => (g.Key, g.Sum(x => x.Quantity)))
                .ToList();
            return Task.FromResult(pool);
        }
    }
}
