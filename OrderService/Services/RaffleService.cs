using OrderService.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace OrderService.Services
{
    public class RaffleService : IRaffleService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IWinnerRepository _winnerRepository;
        private readonly IWinnerService _winnerService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RaffleService> _logger;

        public RaffleService(
            IOrderRepository orderRepository,
            IWinnerRepository winnerRepository,
            IWinnerService winnerService,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<RaffleService> logger)
        {
            _orderRepository = orderRepository;
            _winnerRepository = winnerRepository;
            _winnerService = winnerService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RaffleWinnerResult?> RunRaffleAsync(int giftId)
        {
            if (await _winnerRepository.IsGiftAlreadyWonAsync(giftId))
            {
                _logger.LogWarning("Gift {GiftId} already has a winner", giftId);
                throw new BusinessException("מתנה זו כבר הוגרלה ויש לה זוכה");
            }

            var pool = await _orderRepository.GetRafflePoolAsync(giftId);
            if (pool.Count == 0)
            {
                _logger.LogWarning("No confirmed tickets for gift {GiftId}", giftId);
                return null;
            }

            _logger.LogInformation("Raffle pool for gift {GiftId}: {Total} tickets across {Users} users",
                giftId, pool.Sum(x => x.Item2), pool.Count);

            var winnerUserId = SelectWeightedWinner(pool);
            var winner = await _winnerService.CreateWinnerAsync(giftId, winnerUserId);

            _logger.LogInformation("Winner selected for gift {GiftId}: UserId={UserId}", giftId, winnerUserId);

            _ = SendNotificationAsync(winner.UserId, winner.GiftId);

            return new RaffleWinnerResult { GiftId = winner.GiftId, UserId = winner.UserId };
        }

        private static int SelectWeightedWinner(List<(int UserId, int Quantity)> pool)
        {
            var expanded = pool.SelectMany(p => Enumerable.Repeat(p.UserId, p.Quantity)).ToList();
            return expanded[Random.Shared.Next(expanded.Count)];
        }

        private async Task SendNotificationAsync(int userId, int giftId)
        {
            try
            {
                var baseUrl = _configuration["Services:NotificationService"];
                if (string.IsNullOrEmpty(baseUrl)) return;

                var payload = new WinnerNotificationDto
                {
                    To = $"user-{userId}@placeholder.local",
                    Subject = "מזל טוב! זכית בהגרלה!",
                    Message = $"זכית במתנה מספר {giftId}"
                };

                var client = _httpClientFactory.CreateClient();
                const string correlationHeader = "x-correlation-id";
                var correlationId = _httpContextAccessor.HttpContext?.Request.Headers[correlationHeader].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    client.DefaultRequestHeaders.Remove(correlationHeader);
                    client.DefaultRequestHeaders.Add(correlationHeader, correlationId);
                }

                await client.PostAsJsonAsync($"{baseUrl}/api/notification/send", payload);
            }
            catch
            {
                // notification failure must not fail the raffle
            }
        }
    }
}
