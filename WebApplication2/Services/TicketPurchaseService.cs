using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.BLL;
using WebApplication2.DAL;
using WebApplication2.Models;
using WebApplication2.Models.DTO;

namespace WebApplication2.Services
{
    public interface ITicketPurchaseService
    {
        Task<int> PlaceOrderAsync(OrderDTO dto);
    }

    public class TicketPurchaseService : ITicketPurchaseService
    {
        private readonly IOrderDal _orderDal;
        private readonly IGiftDal _giftDal;
        private readonly IWinnerDAL _winnerDal;
        private readonly IRedisInventoryService _redisInventory;
        private readonly ISalesSummaryCacheService _salesSummaryCache;

        public TicketPurchaseService(
            IOrderDal orderDal,
            IGiftDal giftDal,
            IWinnerDAL winnerDal,
            IRedisInventoryService redisInventory,
            ISalesSummaryCacheService salesSummaryCache)
        {
            _orderDal = orderDal ?? throw new ArgumentNullException(nameof(orderDal));
            _giftDal = giftDal ?? throw new ArgumentNullException(nameof(giftDal));
            _winnerDal = winnerDal ?? throw new ArgumentNullException(nameof(winnerDal));
            _redisInventory = redisInventory ?? throw new ArgumentNullException(nameof(redisInventory));
            _salesSummaryCache = salesSummaryCache ?? throw new ArgumentNullException(nameof(salesSummaryCache));
        }

        public async Task<int> PlaceOrderAsync(OrderDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            foreach (var itemDto in dto.OrderItems)
            {
                bool isAlreadyWon = await _winnerDal.IsGiftAlreadyWonAsync(itemDto.GiftId);
                if (isAlreadyWon)
                {
                    throw new BusinessException("לא ניתן לרכוש כרטיסים למתנה שכבר הוגרלה");
                }
            }

            var gifts = await _giftDal.GetAll();
            decimal totalSum = 0m;
            var orderTickets = new List<OrderTicketModel>();
            var reservedItems = new List<(int GiftId, int Quantity)>();

            try
            {
                foreach (var itemDto in dto.OrderItems)
                {
                    var gift = gifts.FirstOrDefault(g => g.Id == itemDto.GiftId);
                    if (gift == null)
                        throw new BusinessException($"לא נמצאה מתנה עם מזהה {itemDto.GiftId}");

                    totalSum += gift.TicketPrice * itemDto.Quantity;

                    var reservation = await _redisInventory.ReserveTicketQuantityAsync(itemDto.GiftId, itemDto.Quantity);
                    if (reservation < 0)
                        throw new BusinessException($"אין מספיק כרטיסים זמינים למתנה '{gift.Name}'");

                    reservedItems.Add((itemDto.GiftId, itemDto.Quantity));
                    orderTickets.Add(new OrderTicketModel
                    {
                        GiftId = gift.Id,
                        Quantity = itemDto.Quantity
                    });
                }

                var newOrder = new OrderModel
                {
                    UserId = dto.UserId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = (double)totalSum,
                    IsDraft = dto.IsDraft,
                    OrderItems = orderTickets
                };

                var orderId = await _orderDal.AddOrder(newOrder);
                await _salesSummaryCache.InvalidateAsync();
                return orderId;
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
    }
}
