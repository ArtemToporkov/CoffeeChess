using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Events;

namespace CoffeeChess.Domain.Entities;

public class GameState
{
    public string GameId { get; init; }
    public string WhitePlayerId { get; init; }
    public string BlackPlayerId { get; init; }
    public bool IsOver { get; init; }
    public TimeSpan WhiteTimeLeft { get; init; }
    public TimeSpan BlackTimeLeft { get; init; }
    public TimeSpan Increment { get; init; }
    public DateTime LastTimeUpdate { get; init; }
    public PlayerColor CurrentPlayerColor { get; init; }
    public PlayerColor? PlayerWithDrawOffer { get; init; }
    public string CurrentFenPosition { get; init; }
    public IReadOnlyCollection<string> SanMovesHistory { get; init; }
}