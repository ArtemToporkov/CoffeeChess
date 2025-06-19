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
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    public ChessGame ChessGame { get; set; } = new();
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();

    public bool TryMove(string playerId, string from, string to, string? promotion)
    {
        var isWhiteToMove = ChessGame.CurrentPlayer == Player.White;
        var currentPlayerId = isWhiteToMove ? WhitePlayerId : BlackPlayerId;

        if (playerId != currentPlayerId || 
            (currentPlayerId == WhitePlayerId && WhiteTimeLeft < TimeSpan.Zero) ||
            (currentPlayerId == BlackPlayerId && BlackTimeLeft < TimeSpan.Zero))
            return false;

        var promotionChar = promotion?[0];
        var move = new Move(new(from), new(to), 
            ChessGame.CurrentPlayer, promotionChar);

        if (ChessGame.MakeMove(move, true) is MoveType.Invalid)
            return false;

        ValidateTime(isWhiteToMove);
        return true;
    }

    private void ValidateTime(bool isWhiteTurn)
    {
        var deltaTime = DateTime.UtcNow - LastMoveTime;
        LastMoveTime = DateTime.UtcNow;
        if (isWhiteTurn)
        {
            WhiteTimeLeft -= deltaTime;
            WhiteTimeLeft += Increment;
        }
        else
        {
            BlackTimeLeft -= deltaTime;
            BlackTimeLeft += Increment;  
        }
    }
}