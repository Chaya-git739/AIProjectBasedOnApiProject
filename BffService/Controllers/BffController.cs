using Microsoft.AspNetCore.Mvc;
using BffService.Services;
using BffService.DTOs;

namespace BffService.Controllers;

[ApiController]
[Route("api/bff/orders")]
public class BffController : ControllerBase
{
    private readonly IAggregationService _agg;

    public BffController(IAggregationService agg) => _agg = agg;

    [HttpGet("{orderId}/details")]
    public async Task<IActionResult> GetOrderDetails(string orderId)
    {
        if (!Request.Headers.TryGetValue("x-correlation-id", out var correlationValues) || string.IsNullOrWhiteSpace(correlationValues))
        {
            var error = new StandardError("MISSING_HEADER", "x-correlation-id header is required", null, DateTime.UtcNow) { StatusCode = 400 };
            return BadRequest(error);
        }

        var correlationId = correlationValues.ToString();

        var result = await _agg.GetOrderDetailsAsync(orderId, correlationId);
        return result.Match<IActionResult>(
            dto => Ok(dto),
            err => StatusCode(err.StatusCode, err.Body)
        );
    }
}
