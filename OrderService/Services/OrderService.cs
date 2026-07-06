using OrderService.Models;
using OrderService.Models.DTO;

namespace OrderService.Services
{
    public class OrderService : IOrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICatalogServiceClient _catalogClient;
        private readonly IRedisInventoryService _redisInventory;

        public OrderService(
            IOrderRepository orderRepository,
            ICatalogServiceClient catalogClient,
            IRedisInventoryService redisInventory)
        {
            _orderRepository = orderRepository;
            _catalogClient = catalogClient;
            _redisInventory = redisInventory;
        }

        public async Task<int> PlaceOrderAsync(OrderDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.UserId <= 0) throw new BusinessException("משתמש לא חוקי");
            if (dto.OrderItems == null || dto.OrderItems.Count == 0)
            {
                throw new BusinessException("סל הקניות ריק");
            }

            var invalidItem = dto.OrderItems.FirstOrDefault(i => i.GiftId <= 0 || i.Quantity <= 0);
            if (invalidItem != null)
            {
                throw new BusinessException("GiftId ו-Quantity חייבים להיות גדולים מאפס");
            }

            var reservedItems = new List<(int GiftId, int Quantity)>();

            try
            {
                foreach (var item in dto.OrderItems)
                {
                    var reservation = await _redisInventory.ReserveTicketQuantityAsync(item.GiftId, item.Quantity);
                    if (reservation < 0)
                    {
                        throw new BusinessException($"אין מספיק מלאי זמין למתנה {item.GiftId}");
                    }

                    reservedItems.Add((item.GiftId, item.Quantity));
                }

                var order = new OrderModel
                {
                    UserId = dto.UserId,
                    IsDraft = dto.IsDraft,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = dto.OrderItems.Sum(i => i.Quantity),
                    OrderItems = dto.OrderItems.Select(i => new OrderTicketModel
                    {
                        GiftId = i.GiftId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                return await _orderRepository.AddOrderAsync(order);
            }
            catch
            {
                foreach (var reserved in reservedItems)
                {
                    await _redisInventory.ReleaseTicketQuantityAsync(reserved.GiftId, reserved.Quantity);
                }

                throw;
            }
        }

        public async Task<OrderDetailsSourceDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            return new OrderDetailsSourceDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(i => new OrderItemDTO
                {
                    GiftId = i.GiftId,
                    Quantity = i.Quantity
                }).ToList()
            };
        }

        public async Task<List<PurchaserDetailsDto>> GetPurchasersForGiftAsync(int giftId)
        {
            var gift = await _catalogClient.GetGiftByIdAsync(giftId);
            var giftName = gift?.Name ?? $"Gift {giftId}";

            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders
                .SelectMany(o => o.OrderItems.Where(i => i.GiftId == giftId).Select(i => new PurchaserDetailsDto
                {
                    CustomerName = giftName,
                    Email = string.Empty,
                    TicketsCount = i.Quantity
                }))
                .ToList();
        }

        public async Task<List<OrderDTO>> GetUserHistoryAsync(int userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                IsDraft = o.IsDraft,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(i => new OrderItemDTO
                {
                    GiftId = i.GiftId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                IsDraft = o.IsDraft,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(i => new OrderItemDTO
                {
                    GiftId = i.GiftId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task ConfirmOrderAsync(int orderId)
        {
            var success = await _orderRepository.ConfirmOrderAsync(orderId);
            if (!success)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }
        }

        public async Task RemoveOrderItemAsync(int orderId, int giftId)
        {
            var success = await _orderRepository.RemoveItemAsync(orderId, giftId);
            if (!success)
            {
                throw new BusinessException("פריט לא נמצא בהזמנה");
            }
        }

        public async Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            await _orderRepository.AddItemToOrderAsync(orderId, giftId, quantity);
        }
    }
}
