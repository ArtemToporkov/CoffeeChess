namespace CoffeeChess.Domain.Games.Enums;

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