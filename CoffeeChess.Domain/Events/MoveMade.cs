namespace CoffeeChess.Domain.Events;

public class MoveMade(string newPgn) : IDomainEvent
{
    public string NewPgn { get; } = newPgn;
}