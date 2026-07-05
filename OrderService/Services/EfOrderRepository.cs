using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services
{
    public class EfOrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public EfOrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddOrderAsync(OrderModel order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }

        public async Task<List<OrderModel>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        public async Task<List<OrderModel>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<OrderModel?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> ConfirmOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.IsDraft = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            if (giftId <= 0)
            {
                throw new BusinessException("GiftId חייב להיות גדול מאפס");
            }
            if (quantity <= 0)
            {
                throw new BusinessException("Quantity חייב להיות גדול מאפס");
            }

            order.OrderItems.Add(new OrderTicketModel { GiftId = giftId, Quantity = quantity });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveItemAsync(int orderId, int giftId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var item = order.OrderItems.FirstOrDefault(i => i.GiftId == giftId);
            if (item == null) return false;

            order.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<(int UserId, int Quantity)>> GetRafflePoolAsync(int giftId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDraft && o.OrderItems.Any(i => i.GiftId == giftId))
                .SelectMany(o => o.OrderItems
                    .Where(i => i.GiftId == giftId)
                    .Select(i => new { o.UserId, i.Quantity }))
                .GroupBy(x => x.UserId)
                .Select(g => new ValueTuple<int, int>(g.Key, g.Sum(x => x.Quantity)))
                .ToListAsync();
        }
    }
}
