using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication2.Models;
using WebApplication2.Models.DTO;

namespace WebApplication2.DAL
{
    public interface IOrderDal
    {
        Task<int> AddOrder(OrderModel order);
        Task<List<PurchaserDetailsDto>> GetPurchasersByGiftId(int giftId);
        Task<List<OrderModel>> GetUserOrders(int userId);
        Task<List<OrderModel>> GetAllOrders();
        Task<bool> HasOrdersForGift(int giftId);
        /// <summary>בדוק אם קיימות רכישות מאושרות (לא טיוטה) למתנה זו</summary>
        Task<bool> HasConfirmedOrdersForGift(int giftId);
        /// <summary>בדוק אם למשתמש קיימות הזמנות מאושרות</summary>
        Task<bool> HasConfirmedOrdersForUserAsync(int userId);
        Task<bool> ConfirmOrderAsync(int orderId);
        Task<OrderModel> GetOrderByIdAsync(int orderId);
        Task<bool> RemoveOrderItemAsync(int orderId, int giftId);
        Task<int> GetConfirmedOrdersCountAsync();
        Task<int> GetTotalTicketsSoldAsync();
        Task AddItemToOrderAsync(int orderId, int giftId, int quantity);
        /// <summary>Get raffle pool for a gift (UserId and Quantity pairs for confirmed orders)</summary>
        Task<List<(int UserId, int Quantity)>> GetRafflePoolByGiftIdAsync(int giftId);
    }
}