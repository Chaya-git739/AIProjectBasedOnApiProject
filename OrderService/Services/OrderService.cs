using OrderService.Models;
using OrderService.Models.DTO;
using OrderService.Messaging;
using Microsoft.AspNetCore.Http;

namespace OrderService.Services
{
    public class OrderService : IOrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICatalogServiceClient _catalogClient;
        private readonly IRabbitMqPublisher _publisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICatalogServiceClient catalogClient,
            IRabbitMqPublisher publisher,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _catalogClient = catalogClient;
            _publisher = publisher;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

            var orderId = await _orderRepository.AddOrderAsync(order);

            const string correlationHeader = "x-correlation-id";
            var correlationId = _httpContextAccessor.HttpContext?.Request.Headers[correlationHeader].FirstOrDefault()
                ?? string.Empty;

            var evt = new OrderPlacedEvent
            {
                CorrelationId = correlationId,
                OrderId = orderId,
                UserId = dto.UserId,
                Items = dto.OrderItems.Select(i => new OrderPlacedItemEvent
                {
                    GiftId = i.GiftId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _publisher.PublishAsync("order.placed", evt);
            _logger.LogInformation("OrderPlaced event published for order {OrderId}", orderId);

            return orderId;
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
                _logger.LogWarning("[Saga] ConfirmOrderAsync skipped because order not found. OrderId={OrderId}", orderId);
                return;
            }

            _logger.LogInformation("[Saga] ConfirmOrderAsync succeeded. OrderId={OrderId}", orderId);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var success = await _orderRepository.CancelOrderAsync(orderId);
            if (!success)
            {
                _logger.LogWarning("[Saga] CancelOrderAsync skipped because order not found. OrderId={OrderId}", orderId);
                return;
            }

            _logger.LogInformation("[Saga] CancelOrderAsync succeeded. OrderId={OrderId}", orderId);
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
