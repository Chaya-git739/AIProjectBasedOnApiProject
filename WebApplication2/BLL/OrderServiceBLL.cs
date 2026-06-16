using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebApplication2.DAL;
using WebApplication2.Models.DTO;
using WebApplication2.Services;

namespace WebApplication2.BLL
{
    public class OrderServiceBLL : IOrderBLL
    {
        private readonly IOrderDal _orderDal;
        private readonly IMapper _mapper;
        private readonly ITicketPurchaseService _ticketPurchaseService;

        public OrderServiceBLL(
            IOrderDal orderDal,
            IMapper mapper,
            ITicketPurchaseService ticketPurchaseService)
        {
            _orderDal = orderDal ?? throw new ArgumentNullException(nameof(orderDal));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ticketPurchaseService = ticketPurchaseService ?? throw new ArgumentNullException(nameof(ticketPurchaseService));
        }

        public async Task<List<PurchaserDetailsDto>> GetPurchasersForGiftAsync(int giftId)
        {
            // אם IOrderDal.GetPurchasersByGiftId מחזיר Task, יש להמתין לו
            return await _orderDal.GetPurchasersByGiftId(giftId);
        }

        public Task<int> PlaceOrderAsync(OrderDTO dto)
        {
            return _ticketPurchaseService.PlaceOrderAsync(dto);
        }

        public async Task<List<OrderDTO>> GetUserHistoryAsync(int userId)
        {
            var orders = await _orderDal.GetUserOrders(userId);
            if (orders == null || !orders.Any()) return new List<OrderDTO>();
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderDal.GetAllOrders();
            if (orders == null || !orders.Any()) return new List<OrderDTO>();
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task ConfirmOrderAsync(int orderId)
        {
            bool success = await _orderDal.ConfirmOrderAsync(orderId);
            if (!success)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }
        }

        public async Task RemoveOrderItemAsync(int orderId, int giftId)
        {
            var order = await _orderDal.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            if (!order.IsDraft)
            {
                throw new BusinessException("לא ניתן לשנות הזמנה לאחר רכישה");
            }

            bool success = await _orderDal.RemoveOrderItemAsync(orderId, giftId);
            if (!success)
            {
                throw new BusinessException("פריט לא נמצא בהזמנה");
            }
        }

        public async Task AddItemToOrderAsync(int orderId, int giftId, int quantity)
        {
            var order = await _orderDal.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new BusinessException("הזמנה לא נמצאה");
            }

            if (!order.IsDraft)
            {
                throw new BusinessException("לא ניתן לשנות הזמנה מאושרת");
            }

            await _orderDal.AddItemToOrderAsync(orderId, giftId, quantity);
        }
    }
}