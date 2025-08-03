using System.ComponentModel;
using CoffeeChess.Domain.Players.Events;
using CoffeeChess.Domain.Players.Exceptions;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
using JetBrains.Annotations;

namespace CoffeeChess.Domain.Players.AggregatesRoots;

public class Player : AggregateRoot<IDomainEvent>
{
    public string Id { get; init; } = null!;
    public string Name { get; private set; } = null!;
    public int Rating { get; private set; }
    
    public Player(string id, string name, int rating)
    {
        Id = id;
        Name = name;
        Rating = rating;
    }

    public void UpdateRating(int newRating)
    {
        if (newRating < 0)
            throw new InvalidRatingException($"Rating \"{newRating}\" should be greater than 0.");
        
        AddDomainEvent(new PlayerRatingChanged(Id, Rating, newRating));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("For EF core only.", error: false)]
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private Player() { }
}
