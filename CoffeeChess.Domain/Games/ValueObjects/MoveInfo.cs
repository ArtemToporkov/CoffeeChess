namespace CoffeeChess.Domain.Games.ValueObjects;

public record struct MoveInfo(San San, TimeSpan TimeAfterMove);