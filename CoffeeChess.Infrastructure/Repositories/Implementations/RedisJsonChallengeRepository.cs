using System.Text.Json;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Infrastructure.Serialization;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisJsonChallengeRepository(
    IConnectionMultiplexer redis) : IChallengeRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string ChallengeKeyPrefix = "challenge";
    private readonly JsonSerializerOptions _challengeSerializerOptions = GetChallengeSerializerOptions();
    
    public async Task<Challenge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{ChallengeKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Challenge>(redisValue!, _challengeSerializerOptions);
    }

    public async Task AddAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        var serializedChallenge = JsonSerializer.Serialize(challenge, _challengeSerializerOptions);
        await _database.StringSetAsync(
            $"{ChallengeKeyPrefix}:{challenge.PlayerId}", serializedChallenge, when: When.NotExists);
    }

    public async Task DeleteAsync(Challenge challenge, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChallengeKeyPrefix}:{challenge.PlayerId}");

    public async Task SaveChangesAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(challenge, _challengeSerializerOptions);
        await _database.StringSetAsync($"{ChallengeKeyPrefix}:{challenge.PlayerId}", serializedGame);
    }

    public IEnumerable<Challenge> GetAll()
    {
        // TODO: don't use server.Keys because it freezes the server
        
        var server = _database.Multiplexer
            .GetServer(_database.Multiplexer.GetEndPoints().First());

        foreach (var key in server.Keys(pattern: $"{ChallengeKeyPrefix}:*"))
        {
            var redisValue = _database.StringGet(key);
            Console.WriteLine(redisValue);
            var challenge = JsonSerializer.Deserialize<Challenge>(redisValue!, _challengeSerializerOptions)!;
            Console.WriteLine(JsonSerializer.Serialize(challenge, _challengeSerializerOptions));
            if (!redisValue.IsNullOrEmpty)
                yield return JsonSerializer.Deserialize<Challenge>(redisValue!, _challengeSerializerOptions)!;
        }
    }

    private static JsonSerializerOptions GetChallengeSerializerOptions()
        => new()
        {
            Converters =
            {
                new ConstructorBasedConverter<Challenge>(),
                new ConstructorBasedConverter<ChallengeSettings>(),
                new ConstructorBasedConverter<TimeControl>(),
                new ConstructorBasedConverter<EloRatingPreference>()
            },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}