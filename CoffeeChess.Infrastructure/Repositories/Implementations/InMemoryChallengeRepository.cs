using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Entities;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class InMemoryChallengeRepository : IChallengeRepository
{
    private readonly ConcurrentDictionary<string, GameChallenge> _challenges = new();
    
    public Task<GameChallenge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = _challenges.TryGetValue(id, out var chat);
        return Task.FromResult(chat);
    }

    public Task AddAsync(GameChallenge challenge, CancellationToken cancellationToken = default)
    {
        _challenges.TryAdd(challenge.PlayerId, challenge);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(GameChallenge challenge, CancellationToken cancellationToken = default)
    {
        _challenges.TryRemove(challenge.PlayerId, out _);
        return Task.CompletedTask;
    }
    
    public IEnumerable<GameChallenge> GetAll() => _challenges.Values;

    public Task SaveChangesAsync(GameChallenge chat, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}