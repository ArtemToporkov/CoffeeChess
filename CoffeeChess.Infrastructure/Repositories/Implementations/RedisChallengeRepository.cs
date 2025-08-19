using System.Text.Json;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisChallengeRepository(
    IConnectionMultiplexer redis) : IChallengeRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string ChallengeKeyPrefix = "challenge";
    
    public async Task<GameChallenge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{ChallengeKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<GameChallenge>(redisValue!);
    }

    public async Task AddAsync(GameChallenge chat, CancellationToken cancellationToken = default)
    {
        var serializedGameChallenge = JsonSerializer.Serialize(chat);
        await _database.StringSetAsync($"{ChallengeKeyPrefix}:{chat.PlayerId}", serializedGameChallenge, when: When.NotExists);
    }

    public async Task DeleteAsync(GameChallenge chat, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChallengeKeyPrefix}:{chat.PlayerId}");

    public async Task SaveChangesAsync(GameChallenge chat, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(chat);
        await _database.StringSetAsync($"{ChallengeKeyPrefix}:{chat.PlayerId}", serializedGame);
    }

    public IEnumerable<GameChallenge> GetAll()
    {
        // TODO: don't use server.Keys because it freezes the server
        
        var server = _database.Multiplexer
            .GetServer(_database.Multiplexer.GetEndPoints().First());

        foreach (var key in server.Keys(pattern: $"{ChallengeKeyPrefix}:*"))
        {
            var redisValue = _database.StringGet(key);
            if (!redisValue.IsNullOrEmpty)
                yield return JsonSerializer.Deserialize<GameChallenge>(redisValue!)!;
        }
    }
}