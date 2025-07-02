using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameResultCalculatedNotification : INotification
{
    public PlayerInfoModel FirstPlayer { get; init; }
    public PlayerInfoModel SecondPlayer { get; init; }
    public GameResultPayloadModel GameResultPayloadForFirst { get; init; }
    public GameResultPayloadModel GameResultPayloadForSecond { get; init; }
}