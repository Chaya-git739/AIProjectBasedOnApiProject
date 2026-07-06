using System.Text.Json;

namespace BffService.Services;

public class OrderClient : IOrderClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public OrderClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<HttpClientResult<OrderServiceContract.OrderDto>> GetOrderAsync(string orderId, string correlationId, CancellationToken ct)
    {
        var baseUrl = _config["Services:OrderService"] ?? "http://orderservice:80";
        var requestUri = new Uri(new Uri(baseUrl), $"api/Order/{orderId}");
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("x-correlation-id", correlationId);
        request.Headers.Add("Accept", "application/json");

        try
        {
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new HttpClientResult<OrderServiceContract.OrderDto>(default, false, (int)response.StatusCode, response.ReasonPhrase);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<OrderServiceContract.OrderDto>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
            return new HttpClientResult<OrderServiceContract.OrderDto>(payload, true, (int)response.StatusCode);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return new HttpClientResult<OrderServiceContract.OrderDto>(default, false, 408, ex.Message);
        }
        catch (Exception ex)
        {
            return new HttpClientResult<OrderServiceContract.OrderDto>(default, false, 500, ex.Message);
        }
    }
}
