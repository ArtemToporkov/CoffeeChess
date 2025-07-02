using CoffeeChess.Core.Models;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameDrawnNotification : INotification
{
    public PlayerInfoModel WhitePlayerInfo { get; init; }
    public PlayerInfoModel BlackPlayerInfo { get; init; }
    
    public string DrawReason { get; init; }
}