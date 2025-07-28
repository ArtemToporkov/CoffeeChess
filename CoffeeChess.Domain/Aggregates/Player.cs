using CoffeeChess.Domain.Events;
using CoffeeChess.Domain.Events.Game;
using CoffeeChess.Domain.Events.Player;

namespace CoffeeChess.Domain.Aggregates;

public class Player
{
    public string Id { get; init; }
    public string Name { get; private set; }
    public int Rating { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private List<IDomainEvent> _domainEvents;
    
    public Player(string id, string name, int rating)
    {
        Id = id;
        Name = name;
        Rating = rating;
    }

    public void UpdateRating(int newRating)
        => _domainEvents.Add(new RatingChanged(Id, Rating, newRating));
    
    private Player() { }
}
