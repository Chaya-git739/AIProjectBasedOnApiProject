using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using OrderService.Models.DTO;

namespace OrderService.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CatalogServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogServiceClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CatalogServiceClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CatalogGiftDto?> GetGiftByIdAsync(int giftId)
        {
            try
            {
                var baseUrl = _configuration["Services:CatalogService"];
                if (string.IsNullOrEmpty(baseUrl)) return null;

                const string correlationHeader = "x-correlation-id";
                var correlationId = _httpContextAccessor.HttpContext?.Request.Headers[correlationHeader].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    _httpClient.DefaultRequestHeaders.Remove(correlationHeader);
                    _httpClient.DefaultRequestHeaders.Add(correlationHeader, correlationId);
                }

                return await _httpClient.GetFromJsonAsync<CatalogGiftDto>($"{baseUrl}/api/gift/{giftId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve gift {GiftId} from CatalogService", giftId);
                return null;
            }
        }
    }
}
