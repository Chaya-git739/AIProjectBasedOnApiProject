using System.Text.Json.Serialization;

namespace BffService.DTOs;

public record StandardError(string ErrorCode, string Message, string? CorrelationId, DateTime? TimestampUtc = null)
{
    [JsonIgnore]
    public int StatusCode { get; init; } = 500;
}
