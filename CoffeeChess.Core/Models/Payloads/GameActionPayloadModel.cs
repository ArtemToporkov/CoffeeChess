using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models.Payloads;

public class GameActionPayloadModel
{
    public GameActionType GameActionType { get; set; }
    public string? Message { get; set; }
}