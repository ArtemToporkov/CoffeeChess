namespace CoffeeChess.Domain.Events;

public class MoveFailed(string reason) : IDomainEvent
{
    public string Reason { get; } = reason;
}