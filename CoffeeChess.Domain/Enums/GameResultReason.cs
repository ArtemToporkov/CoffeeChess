namespace CoffeeChess.Domain.Enums;

public enum GameResultReason
{
    WhiteResigns,
    BlackResigns,
    WhiteTimeRanOut,
    BlackTimeRanOut,
    WhiteCheckmates,
    BlackCheckmates,
    Agreement,
    Stalemate,
    Threefold,
    FiftyMovesRule
}