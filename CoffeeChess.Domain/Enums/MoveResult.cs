namespace CoffeeChess.Domain.Enums;

public enum MoveResult
{
    Success,
    Invalid,
    TimeRanOut,
    NotYourTurn,
    Checkmate,
    ThreeFold,
    FiftyMovesRule,
    Stalemate
}