using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using WebApplication2.BLL;
using WebApplication2.Models.DTO;

namespace WebApplication2.Services
{
    public interface ISalesSummaryCacheService
    {
        Task<SalesSummaryDto> GetSalesSummaryAsync();
        Task InvalidateAsync();
    }

    public class SalesSummaryCacheService : ISalesSummaryCacheService
    {
        private const string CacheKey = "dashboard:sales_summary";
        private readonly IDistributedCache _cache;
        private readonly IGiftBLL _giftBll;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SalesSummaryCacheService(IDistributedCache cache, IGiftBLL giftBll)
        {
            _cache = cache;
            _giftBll = giftBll;
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync()
        {
            var cached = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<SalesSummaryDto>(cached, _jsonOptions)!;
            }

            var summary = await _giftBll.GetSalesSummaryAsync();
            var serialized = JsonSerializer.Serialize(summary, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(10)
            };

            await _cache.SetStringAsync(CacheKey, serialized, options);
            return summary;
        }

        public Task InvalidateAsync()
        {
            return _cache.RemoveAsync(CacheKey);
        }
    }
}
