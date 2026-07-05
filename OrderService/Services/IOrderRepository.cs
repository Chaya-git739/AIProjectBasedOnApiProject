using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderRepository
    {
        Task<int> AddOrderAsync(OrderModel order);
        Task<List<OrderModel>> GetAllOrdersAsync();
        Task<List<OrderModel>> GetUserOrdersAsync(int userId);
        Task<OrderModel?> GetOrderByIdAsync(int orderId);
        Task<bool> ConfirmOrderAsync(int orderId);
        Task AddItemToOrderAsync(int orderId, int giftId, int quantity);
        Task<bool> RemoveItemAsync(int orderId, int giftId);
    }
}
