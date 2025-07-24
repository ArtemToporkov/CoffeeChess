using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events;

public class GameResultUpdated(Result result, string whiteReason, string blackReason) : IDomainEvent
{
    public Result Result { get; } = result;
    public string WhiteReason { get; } = whiteReason;
    public string BlackReason { get; } = blackReason;
}