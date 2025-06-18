using System.Collections.Concurrent;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models;

public class GameModel
{
    public string GameId { get; set; }
    public string WhitePlayerId { get; set; }
    public string BlackPlayerId { get; set; }
    public TimeSpan WhiteTimeLeft { get; set; }
    public TimeSpan BlackTimeLeft { get; set; }
    public TimeSpan Increment { get; set; }
    public ChessGame ChessGame { get; set; } = new();
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();
}