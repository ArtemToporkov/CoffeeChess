using CoffeeChess.Domain.Players.Events;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Players.AggregatesRoots;

public class Player
{
    public string Id { get; init; } = null!;
    public string Name { get; private set; } = null!;
    public int Rating { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = [];

    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public Player(string id, string name, int rating)
    {
        Id = id;
        Name = name;
        Rating = rating;
    }

    public void UpdateRating(int newRating)
        => _domainEvents.Add(new PlayerRatingChanged(Id, Rating, newRating));
    
    private Player() { }
}
