using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models.Payloads;

public class GameResultPayloadModel
{
    public GameResultForPlayer Result { get; set; }
    public string? Message { get; set; }
    public int? OldRating { get; set; }
    public int? NewRating { get; set; }
}