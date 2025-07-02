using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models.Payloads;

public class GameResultPayloadModel(GameResultForPlayer result, 
    string? message, int? oldRating, int? newRating)
{
    public GameResultForPlayer Result { get; set; } = result;
    public string? Message { get; set; } = message;
    public int? OldRating { get; set; } = oldRating;
    public int? NewRating { get; set; } = newRating;
}