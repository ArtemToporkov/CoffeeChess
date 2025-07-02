using CoffeeChess.Core.Models;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameDrawnNotification : INotification
{
    public PlayerInfoModel FirstPlayer { get; init; }
    public PlayerInfoModel SecondPlayer { get; init; }
    
    public string DrawReason { get; init; }
}