using System.Text.Json;

namespace BffService.Services;

public class ProductClient : IProductClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public ProductClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<HttpClientResult<CatalogServiceContract.GiftDto>> GetGiftAsync(int giftId, string correlationId, CancellationToken ct)
    {
        var baseUrl = _config["Services:CatalogService"] ?? "http://catalogservice:80";
        var requestUri = new Uri(new Uri(baseUrl), $"api/Gift/{giftId}");
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("x-correlation-id", correlationId);
        request.Headers.Add("Accept", "application/json");

        try
        {
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new HttpClientResult<CatalogServiceContract.GiftDto>(default, false, (int)response.StatusCode, response.ReasonPhrase);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<CatalogServiceContract.GiftDto>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
            return new HttpClientResult<CatalogServiceContract.GiftDto>(payload, true, (int)response.StatusCode);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return new HttpClientResult<CatalogServiceContract.GiftDto>(default, false, 408, ex.Message);
        }
        catch (Exception ex)
        {
            return new HttpClientResult<CatalogServiceContract.GiftDto>(default, false, 500, ex.Message);
        }
    }
}
