using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class BaseGameRepository(IMediator mediator) : IGameRepository
{
    private readonly ConcurrentDictionary<string, Game> _games = new();

    public bool TryGetValue(string id, [NotNullWhen(true)] out Game? game)
    {
        if (_games.TryGetValue(id, out game))
            return true;

        game = null;
        return false;
    }

    public bool TryAdd(string id, Game challenge) => _games.TryAdd(id, challenge);
    
    public bool TryRemove(string id, [NotNullWhen(true)] out Game? removedGame) 
        => _games.TryRemove(id, out removedGame);

    public IEnumerable<(string, Game)> GetAll() 
        => _games.Select(kvp => (kvp.Key, kvp.Value));

    public void SaveChanges(Game game)
    {
        foreach (var @event in game.DomainEvents)
            mediator.Publish(@event);
        game.ClearDomainEvents();
    }

    public IEnumerable<Game> GetActiveGames() => _games.Values
        .Where(g => !g.IsOver);
}