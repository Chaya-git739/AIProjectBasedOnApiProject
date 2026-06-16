using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApplication2.Models.DTO;
using WebApplication2.BLL;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController : ControllerBase
    {
        private readonly IGiftBLL _giftBll;
        private readonly ISalesSummaryCacheService _salesSummaryCache;

        public GiftController(IGiftBLL giftBll, ISalesSummaryCacheService salesSummaryCache)
        {
            _giftBll = giftBll;
            _salesSummaryCache = salesSummaryCache;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] string? name, [FromQuery] string? donorName, [FromQuery] int? minPurchasers)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(donorName) && !minPurchasers.HasValue)
            {
                var allGifts = await _giftBll.GetAllGiftsAsync();
                return Ok(allGifts);
            }
            
            var gifts = await _giftBll.GetGiftsByFilterAsync(name, donorName, minPurchasers);
            return Ok(gifts);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateGiftDTO createGift)
        {
            var gift = new GiftDTO
            {
                Name = createGift.Name,
                Description = createGift.Description,
                TicketPrice = createGift.TicketPrice,
                Category = createGift.Category,
                DonorName = createGift.DonorName
            };
            await _giftBll.AddGiftAsync(gift);
            return Ok("המתנה נוספה בהצלחה");
        }

        [Authorize(Roles = "Manager")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] GiftDTO gift)
        {
            await _giftBll.UpdateGiftAsync(gift);
            return Ok("המתנה עודכנה בהצלחה");
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _giftBll.DeleteGiftAsync(id);
                return Ok("המתנה נמחקה מהמערכת");
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("sorted-by-price")]
        public async Task<IActionResult> GetGiftsByPrice()
        {
            var gifts = await _giftBll.GetGiftsSortedByPriceAsync();
            return Ok(gifts);
        }

        [HttpGet("most-purchased")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMostPurchased()
        {
            var gifts = await _giftBll.GetMostPurchasedGiftsAsync();
            return Ok(gifts);
        }

        [HttpGet("sales-summary")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetSalesSummary()
        {
            var summary = await _salesSummaryCache.GetSalesSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("gifts-with-winners")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetGiftsWithWinners()
        {
            var giftsWithWinners = await _giftBll.GetGiftsWithWinnersAsync();
            return Ok(giftsWithWinners);
        }
    }
}