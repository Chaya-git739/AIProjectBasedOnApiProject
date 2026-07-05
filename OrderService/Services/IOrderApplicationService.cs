using OrderService.Models.DTO;

namespace OrderService.Services
{
    public interface IOrderApplicationService
    {
        Task<int> PlaceOrderAsync(OrderDTO dto);
        Task<List<PurchaserDetailsDto>> GetPurchasersForGiftAsync(int giftId);
        Task<List<OrderDTO>> GetUserHistoryAsync(int userId);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task ConfirmOrderAsync(int orderId);
        Task RemoveOrderItemAsync(int orderId, int giftId);
        Task AddItemToOrderAsync(int orderId, int giftId, int quantity);
    }
}
