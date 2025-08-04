using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Application.Games.ReadModels;

public class CompletedGameReadModel
{
    public string GameId { get; init; }
    public string WhitePlayerName { get; init; }
    public string BlackPlayerName { get; init; }
    public int WhitePlayerRating { get; init; }
    public int BlackPlayerRating { get; init; }
    public GameResult GameResult { get; init; }
    public DateTime PlayedDate { get; init; }
    public string Pgn { get; init; }
}