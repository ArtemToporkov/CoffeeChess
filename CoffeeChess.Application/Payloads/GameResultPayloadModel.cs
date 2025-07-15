using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Application.Payloads;

public class GameResultPayloadModel(GameResultForPlayer result, 
    string? message, int? oldRating, int? newRating)
{
    public GameResultForPlayer Result { get; set; } = result;
    public string? Message { get; set; } = message;
    public int? OldRating { get; set; } = oldRating;
    public int? NewRating { get; set; } = newRating;
}