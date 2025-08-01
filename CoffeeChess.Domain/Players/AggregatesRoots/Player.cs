﻿using CoffeeChess.Domain.Players.Events;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;

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
        => AddDomainEvent(new PlayerRatingChanged(Id, Rating, newRating));
    
    private Player() { }
}
