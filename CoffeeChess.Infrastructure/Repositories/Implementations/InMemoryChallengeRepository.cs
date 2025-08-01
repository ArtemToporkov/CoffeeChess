using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Entities;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class InMemoryChallengeRepository : IChallengeRepository
{
    private readonly ConcurrentDictionary<string, GameChallenge> _challenges = new();
    
    public Task<GameChallenge?> GetByIdAsync(string id)
    {
        _ = _challenges.TryGetValue(id, out var chat);
        return Task.FromResult(chat);
    }

    public Task AddAsync(GameChallenge challenge)
    {
        _challenges.TryAdd(challenge.PlayerId, challenge);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(GameChallenge challenge)
    {
        _challenges.TryRemove(challenge.PlayerId, out _);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<GameChallenge> GetAllAsync()
    {
        throw new NotImplementedException();
    }
    
    public IEnumerable<GameChallenge> GetAll() => _challenges.Values;

    public Task SaveChangesAsync(GameChallenge chat)
    {
        return Task.CompletedTask;
    }
}