using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.DAL;
using WebApplication2.Models.DTO;

namespace WebApplication2.BLL
{
    public class GiftServiceBLL : IGiftBLL
    {
        private readonly IGiftDal _giftDal;
        private readonly IOrderDal _orderDal;
        private readonly ILogger<GiftServiceBLL> _logger;

        public GiftServiceBLL(IGiftDal giftDal, IOrderDal orderDal, ILogger<GiftServiceBLL> logger)
        {
            _giftDal = giftDal ?? throw new ArgumentNullException(nameof(giftDal));
            _orderDal = orderDal ?? throw new ArgumentNullException(nameof(orderDal));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<GiftDTO>> GetAllGiftsAsync()
        {
            var gifts = await _giftDal.GetAll();
            return gifts;
        }

        public async Task<List<GiftDTO>> GetGiftsByFilterAsync(string? name, string? donorName, int? minPurchasers)
        {
            var gifts = await _giftDal.GetByFilter(name, donorName, minPurchasers);
            return gifts;
        }

        public Task<List<GiftDTO>> GetGiftsSortedByPriceAsync() => _giftDal.GetGiftsSortedByPrice();

        public Task<List<GiftDTO>> GetMostPurchasedGiftsAsync() => _giftDal.GetMostPurchasedGifts();

        public async Task AddGiftAsync(GiftDTO gift)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gift.Name))
                {
                    throw new BusinessException("שם המתנה אינו יכול להיות ריק.");
                }
                // 1. בדיקה האם כבר קיימת מתנה עם שם כזה (ללא הבדל בין אותיות גדולות לקטנות)
                var allGifts = await _giftDal.GetAll();
                bool exists = allGifts.Any(g => g.Name.Trim().ToLower() == gift.Name.Trim().ToLower());

                if (exists)
                {
                    throw new BusinessException($"מתנה בשם '{gift.Name}' כבר קיימת במערכת. לא ניתן להוסיף מתנות כפולות.");
                }

                // 2. אם לא קיימת, ממשיכים להוספה רגילה
                await _giftDal.Add(gift);
            }
            catch (BusinessException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding gift");
                throw new Exception("אירעה שגיאה טכנית בעת הוספת המתנה. נא לנסות שוב מאוחר יותר.", ex);
            }
        }
         
        public Task UpdateGiftAsync(GiftDTO gift) => _giftDal.Update(gift);

        public async Task DeleteGiftAsync(int id)
        {
            // בדיקה 1: האם קיימות רכישות מאושרות (confirmed orders)?
            bool hasConfirmedOrders = await _orderDal.HasConfirmedOrdersForGift(id);
            if (hasConfirmedOrders)
                throw new BusinessException("לא ניתן למחוק את המתנה כיוון שכבר יש עבורה רכישות מאושרות שלא ניתן להפר.");

            // בדיקה 2: האם קיימות הזמנות טיוטה (draft orders) עם מתנה זו?
            bool hasDraftOrders = await _orderDal.HasOrdersForGift(id);
            if (hasDraftOrders)
                throw new BusinessException("לא ניתן למחוק את המתנה כיוון שיש הזמנות טיוטה המכילות אותה.");

            // מחיקה רכה (Soft Delete)
            await _giftDal.Delete(id);
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync()
        {
            var totalRevenue = await _giftDal.GetTotalSalesAsync();
            var totalOrders = await _orderDal.GetConfirmedOrdersCountAsync();
            var totalTickets = await _orderDal.GetTotalTicketsSoldAsync();
            var salesPerGift = await _giftDal.GetSalesPerGiftAsync();

            return new SalesSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalTicketsSold = totalTickets,
                SalesPerGift = salesPerGift
            };
        }

        public Task<List<GiftWinnerDto>> GetGiftsWithWinnersAsync() => _giftDal.GetGiftsWithWinnersAsync();
    }
}