using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class RaffleController : ControllerBase
    {
        private readonly IRaffleService _raffleService;

        public RaffleController(IRaffleService raffleService)
        {
            _raffleService = raffleService;
        }

        [HttpPost("run/{giftId}")]
        public async Task<IActionResult> RunRaffle(int giftId)
        {
            var result = await _raffleService.RunRaffleAsync(giftId);
            if (result == null)
                return BadRequest(new { message = "אין כרטיסים למתנה זו" });

            return Ok(new { message = "הגרלה בוצעה בהצלחה", winner = result });
        }
    }
}
