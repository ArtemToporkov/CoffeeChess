using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using MediatR;

namespace CoffeeChess.Web.Notifications;

public class GameEndedNotification : INotification
{
    public GameModel Game { get; init; }
    public PlayerColor Winner { get; init; }
    public string WinReason { get; init; }
    public string LoseReason { get; init; }
}