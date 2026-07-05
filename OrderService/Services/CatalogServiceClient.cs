using System.Net.Http.Json;
using OrderService.Models.DTO;

namespace OrderService.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CatalogServiceClient> _logger;

        public CatalogServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<CatalogServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CatalogGiftDto?> GetGiftByIdAsync(int giftId)
        {
            try
            {
                var baseUrl = _configuration["Services:CatalogService"];
                if (string.IsNullOrEmpty(baseUrl)) return null;

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
