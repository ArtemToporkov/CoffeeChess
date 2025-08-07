namespace CoffeeChess.Domain.Games.Enums;

public enum GameResultReason
{
    OpponentResigned,
    OpponentTimeRanOut,
    Checkmate,
    Agreement,
    Stalemate,
    Threefold,
    FiftyMovesRule
}