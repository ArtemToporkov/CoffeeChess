using CoffeeChess.Core.Models;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameDrawnNotification : INotification
{
    public GameModel Game { get; init; }
    public string DrawReason { get; init; }
}