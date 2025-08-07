using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Application.Games.ReadModels;

public class CompletedGameReadModel
{
    public string GameId { get; init; }
    
    public string WhitePlayerId { get; init; }
    public string WhitePlayerName { get; init; }
    public int WhitePlayerRating { get; init; }
    public int WhitePlayerNewRating { get; init; }
    
    public string BlackPlayerId { get; init; }
    public string BlackPlayerName { get; init; }
    public int BlackPlayerRating { get; init; }
    public int BlackPlayerNewRating { get; init; }
    
    public int Minutes { get; init; }
    public int Increment { get; init; }
    
    public GameResult GameResult { get; init; }
    public GameResultReason GameResultReason { get; init; }
    public DateTime PlayedDate { get; init; }
    
    public List<SanMove> SanMovesHistory { get; init; }
}