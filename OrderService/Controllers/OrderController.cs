using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models.DTO;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderApplicationService _orderBll;

        public OrderController(IOrderApplicationService orderBll)
        {
            _orderBll = orderBll;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] OrderDTO orderDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (orderDto.OrderItems == null || orderDto.OrderItems.Count == 0)
            {
                return BadRequest("סל הקניות ריק");
            }

            var invalidItem = orderDto.OrderItems.FirstOrDefault(i => i.GiftId <= 0 || i.Quantity <= 0);
            if (invalidItem != null)
            {
                return BadRequest("GiftId ו-Quantity חייבים להיות גדולים מאפס");
            }

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    return Unauthorized("משתמש לא מזוהה או לא חוקי");
                }

                orderDto.UserId = userId;
                int orderId = await _orderBll.PlaceOrderAsync(orderDto);
                return Ok(new { Message = "ההזמנה בוצעה בהצלחה!", OrderId = orderId });
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"אירעה שגיאה בעיבוד ההזמנה: {ex.Message}");
            }
        }

        [HttpGet("purchasers/{giftId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPurchasers(int giftId)
        {
            var purchasers = await _orderBll.GetPurchasersForGiftAsync(giftId);
            if (purchasers == null || purchasers.Count == 0)
            {
                return NotFound("לא נמצאו רוכשים למתנה זו");
            }
            return Ok(purchasers);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderBll.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception)
            {
                return StatusCode(500, "אירעה שגיאה בטעינת ההזמנות");
            }
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetById(int orderId)
        {
            try
            {
                var order = await _orderBll.GetOrderByIdAsync(orderId);
                return order == null ? NotFound("הזמנה לא נמצאה") : Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(500, "אירעה שגיאה בטעינת ההזמנה");
            }
        }

        [HttpGet("user/history")]
        public async Task<IActionResult> GetUserOrderHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    return Unauthorized("משתמש לא מזוהה או לא חוקי");
                }

                var orders = await _orderBll.GetUserHistoryAsync(userId);
                if (orders == null || orders.Count == 0)
                {
                    return NotFound("לא נמצאו הזמנות עבור המשתמש הנוכחי");
                }
                return Ok(orders);
            }
            catch (Exception)
            {
                return StatusCode(500, "אירעה שגיאה בעת fetch היסטוריית ההזמנה");
            }
        }

        [HttpPost("confirm/{orderId}")]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            try
            {
                await _orderBll.ConfirmOrderAsync(orderId);
                return Ok("ההזמנה אושרה בהצלחה");
            }
            catch (BusinessException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "אירעה שגיאה באישור ההזמנה");
            }
        }

        [HttpPost("{orderId}/add-item")]
        [Authorize]
        public async Task<IActionResult> AddItemToOrder(int orderId, [FromBody] AddItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                await _orderBll.AddItemToOrderAsync(orderId, request.GiftId, request.Quantity);
                return Ok("הפריט נוסף להזמנה");
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "אירעה שגיאה");
            }
        }
    }

    public class AddItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "GiftId חייב להיות גדול מאפס")]
        public int GiftId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity חייב להיות גדול מאפס")]
        public int Quantity { get; set; }
    }
}
