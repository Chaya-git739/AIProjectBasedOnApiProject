using StackExchange.Redis;

namespace OrderService.Services
{
    public interface IRedisInventoryService
    {
        Task<long> ReserveTicketQuantityAsync(int raffleId, int quantity);
        Task<long> ReleaseTicketQuantityAsync(int raffleId, int quantity = 1);
        Task<int?> GetAvailableTicketCountAsync(int raffleId);
    }

    public class RedisInventoryService : IRedisInventoryService
    {
        private readonly IDatabase _db;

        private static readonly LuaScript ReserveQuantityScript = LuaScript.Prepare(@"
local current = redis.call('GET', KEYS[1])
if not current then return -1 end
local requested = tonumber(ARGV[1])
current = tonumber(current)
if current < requested then return -1 end
return redis.call('DECRBY', KEYS[1], requested)
");

        public RedisInventoryService(IConnectionMultiplexer multiplexer)
        {
            _db = multiplexer.GetDatabase();
        }

        public async Task<long> ReserveTicketQuantityAsync(int raffleId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            var key = $"raffle:{raffleId}:available_count";
            var script = ReserveQuantityScript.ToString();
            var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { quantity });

            return (long)result;
        }

        public Task<long> ReleaseTicketQuantityAsync(int raffleId, int quantity = 1)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            var key = $"raffle:{raffleId}:available_count";
            return _db.StringIncrementAsync(key, quantity);
        }

        public async Task<int?> GetAvailableTicketCountAsync(int raffleId)
        {
            var value = await _db.StringGetAsync($"raffle:{raffleId}:available_count");
            return value.IsNull ? null : (int?)value;
        }
    }
}
