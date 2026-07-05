using OrderService.Models.DTO;
using OrderService.Services;

namespace OrderService.Services
{
    public class OrderApplicationService : IOrderApplicationService
    {
        private static readonly List<OrderDTO> Orders = new();
        private static int NextOrderId = 1;

        public Task<int> PlaceOrderAsync(OrderDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var order = new OrderDTO
            {
                Id = NextOrderId++,
                UserId = dto.UserId,
                UserName = dto.UserName,
                UserEmail = dto.UserEmail,
                OrderDate = DateTime.UtcNow,
                IsDraft = dto.IsDraft,
                TotalAmount = dto.OrderItems.Sum(i => i.Quantity),
                OrderItems = dto.OrderItems
            };

            Orders.Add(order);
            return Task.FromResult(order.Id);
        }

        public Task<List<PurchaserDetailsDto>> GetPurchasersForGiftAsync(int giftId)
        {
            var purchasers = Orders
                .SelectMany(o => o.OrderItems.Where(i => i.GiftId == giftId).Select(i => new PurchaserDetailsDto
                {
                    CustomerName = o.UserName ?? "Unknown",
                    Email = o.UserEmail ?? string.Empty,
                    TicketsCount = i.Quantity
                }))
                .ToList();

            return Task.FromResult(purchasers);
        }

        public Task<List<OrderDTO>> GetUserHistoryAsync(int userId)
        {
            return Task.FromResult(Orders.Where(o => o.UserId == userId).ToList());
        }

        public Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            return Task.FromResult(Orders.ToList());
        }

        public Task ConfirmOrderAsync(int orderId)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            order.IsDraft = false;
            return Task.CompletedTask;
        }

        public Task RemoveOrderItemAsync(int orderId, int giftId)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            order.OrderItems = order.OrderItems.Where(i => i.GiftId != giftId).ToList();
            return Task.CompletedTask;
        }

        public Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            order.OrderItems.Add(new OrderItemDTO { GiftId = giftId, Quantity = quantity });
            return Task.CompletedTask;
        }
    }
}
