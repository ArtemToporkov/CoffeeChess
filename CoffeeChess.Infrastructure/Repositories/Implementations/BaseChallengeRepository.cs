using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Repositories.Interfaces;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class BaseChallengeRepository : IChallengeRepository
{
    private readonly ConcurrentDictionary<string, GameChallenge> _challenges = new();

    public bool TryGetValue(string id, [NotNullWhen(true)] out GameChallenge? challenge)
    {
        if (_challenges.TryGetValue(id, out challenge))
            return true;

        challenge = null;
        return false;
    }

    public bool TryAdd(string id, GameChallenge challenge) => _challenges.TryAdd(id, challenge);
    
    public bool TryRemove(string id, [NotNullWhen(true)] out GameChallenge? removedChallenge) 
        => _challenges.TryRemove(id, out removedChallenge);

    public IEnumerable<(string, GameChallenge)> GetAll()
        => _challenges.Select(kvp => (kvp.Key, kvp.Value));
    
}