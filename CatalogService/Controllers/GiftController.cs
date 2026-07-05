using Microsoft.AspNetCore.Mvc;
using CatalogService.Models.DTO;
using CatalogService.Services;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController : ControllerBase
    {
        private readonly IGiftService _service;
        public GiftController(IGiftService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GiftDto gift) => Ok(await _service.AddAsync(gift));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GiftDto gift)
        {
            var updated = await _service.UpdateAsync(id, gift);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
