using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameResultCalculatedNotification : INotification
{
    public GameModel Game { get; init; }
    public GameResultPayloadModel GameResultPayloadForWhite { get; init; }
    public GameResultPayloadModel GameResultPayloadForBlack { get; init; }
}