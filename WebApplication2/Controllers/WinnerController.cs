using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.DAL;
using WebApplication2.Models;
using WebApplication2.BLL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
// controller for all the crud actions on winner
namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WinnerController : ControllerBase
    {
        private readonly IWinnerBLL _winnerBll;
        
        public WinnerController(IWinnerBLL winnerBll) {
            _winnerBll = winnerBll;
        }
        // GET: api/winner
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task <List<WinnerModel>> Get()   
        {
            return await _winnerBll.GetAllWinners();
        }

        // GET api/winner/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<WinnerModel> Get(int id)
        {
            return await _winnerBll.GetWinnerById(id);
        }

        // POST api/winner
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromBody] WinnerModel winnerModel)
        {
            try
            {
                await _winnerBll.AddWinnerAndNotifyAsync(winnerModel);
                
                return Ok("זוכה נוסף בהצלחה ומייל נשלח");
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה: {ex.Message}");
            }
        }
  

        // PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // POST api/winner/send-email/{winnerId}
        [HttpPost("send-email/{winnerId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SendWinnerEmail(int winnerId)
        {
            try
            {
                var winner = await _winnerBll.GetWinnerById(winnerId);
                if (winner == null)
                {
                    return NotFound("זוכה לא נמצא");
                }

                await _winnerBll.NotifyWinnerAsync(winner);

                return Ok("מייל נשלח בהצלחה");
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליחת מייל: {ex.Message}");
            }
        }

        // DELETE api/winner/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task Delete(int id)
        {
            await _winnerBll.DeleteWinner(id);
            
        }
    }
}
