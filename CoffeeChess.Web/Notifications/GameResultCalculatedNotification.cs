using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameResultCalculatedNotification : INotification
{
    public PlayerInfoModel WhitePlayerInfo { get; init; }
    public PlayerInfoModel BlackPlayerInfo { get; init; }
    public GameResultPayloadModel GameResultPayloadForWhite { get; init; }
    public GameResultPayloadModel GameResultPayloadForBlack { get; init; }
}