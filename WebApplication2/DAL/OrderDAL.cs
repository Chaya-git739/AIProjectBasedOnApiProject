using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.Models.DTO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.DAL
{
    public class OrderDAL : IOrderDal
    {
        private readonly StoreContext _context;

        public OrderDAL(StoreContext context)
        {
            _context = context;
        }

        public async Task<int> AddOrder(OrderModel order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // saves the order and its items
            return order.Id;
        }

        // Read-only: AsNoTracking + projection
        public async Task<List<PurchaserDetailsDto>> GetPurchasersByGiftId(int giftId)
        {
            return await _context.OrderTicket
                .AsNoTracking()
                .Where(t => t.GiftId == giftId && t.Order.IsDraft == true)
                .Select(t => new PurchaserDetailsDto
                {
                    CustomerName = t.Order.User.Name,
                    Email = t.Order.User.Email,
                    TicketsCount = t.Quantity
                })
                .ToListAsync();
        }

        // Read-only: AsNoTracking; project orders with items to avoid loading other columns unnecessarily
        public async Task<List<OrderModel>> GetUserOrders(int userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.IsDraft == true)
                .Select(o => new OrderModel
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    IsDraft = o.IsDraft,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderItems = o.OrderItems.Select(oi => new OrderTicketModel
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        GiftId = oi.GiftId,
                        Quantity = oi.Quantity,
                        Gift = null! // avoid loading full Gift; load separately if caller needs it
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<OrderModel>> GetAllOrders()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Gift)
                .ToListAsync();
        }

        public async Task<bool> HasOrdersForGift(int giftId)
        {
            return await _context.OrderTicket.AnyAsync(t => t.GiftId == giftId);
        }

        /// <summary>
        /// בדוק אם קיימות רכישות מאושרות (לא טיוטה) למתנה זו
        /// </summary>
        public async Task<bool> HasConfirmedOrdersForGift(int giftId)
        {
            return await _context.OrderTicket
                .AnyAsync(t => t.GiftId == giftId && t.Order.IsDraft == false);
        }

        /// <summary>
        /// בדוק אם למשתמש קיימות הזמנות מאושרות (לא טיוטה)
        /// </summary>
        public async Task<bool> HasConfirmedOrdersForUserAsync(int userId)
        {
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.IsDraft == false);
        }

        public async Task<bool> ConfirmOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.IsDraft = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<OrderModel>? GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> RemoveOrderItemAsync(int orderId, int giftId)
        {
            var orderItem = await _context.OrderTicket
                .FirstOrDefaultAsync(ot => ot.OrderId == orderId && ot.GiftId == giftId);
            
            if (orderItem != null)
            {
                _context.OrderTicket.Remove(orderItem);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<int> GetConfirmedOrdersCountAsync()
        {
            return await _context.Orders.CountAsync(o => o.IsDraft == false);
        }

        public async Task<int> GetTotalTicketsSoldAsync()
        {
            return await _context.OrderTicket
                .Where(t => t.Order.IsDraft == false)
                .SumAsync(t => t.Quantity);
        }

        public async Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            var orderItem = new OrderTicketModel
            {
                OrderId = orderId,
                GiftId = giftId,
                Quantity = quantity
            };
            
            _context.OrderTicket.Add(orderItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get raffle pool for a gift (UserId and Quantity pairs for confirmed orders only)
        /// </summary>
        public async Task<List<(int UserId, int Quantity)>> GetRafflePoolByGiftIdAsync(int giftId)
        {
            return await _context.OrderTicket
                .AsNoTracking()
                .Where(ot => ot.GiftId == giftId && ot.Order.IsDraft == false)
                .Select(ot => new { ot.Order.UserId, ot.Quantity })
                .GroupBy(x => x.UserId)
                .Select(g => new ValueTuple<int, int>(g.Key, g.Sum(x => x.Quantity)))
                .ToListAsync();
        }
    }
}