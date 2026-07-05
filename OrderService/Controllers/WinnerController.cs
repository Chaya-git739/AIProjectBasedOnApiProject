using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class WinnerController : ControllerBase
    {
        private readonly IWinnerService _winnerService;

        public WinnerController(IWinnerService winnerService)
        {
            _winnerService = winnerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWinner([FromBody] WinnerRequest request)
        {
            try
            {
                var winner = await _winnerService.CreateWinnerAsync(request.GiftId, request.UserId);
                return Ok(new { message = "זוכה נוסף בהצלחה", winner });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWinners()
        {
            var winners = await _winnerService.GetWinnersAsync();
            return Ok(winners);
        }
    }

    public class WinnerRequest
    {
        public int GiftId { get; set; }
        public int UserId { get; set; }
    }
}
