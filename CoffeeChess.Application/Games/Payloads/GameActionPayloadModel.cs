using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Application.Games.Payloads;

public class GameActionPayloadModel
{
    public GameActionType GameActionType { get; set; }
    public string? Message { get; set; }
}