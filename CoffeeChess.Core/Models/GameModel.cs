using System.Collections.Concurrent;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models;

public class GameModel
{
    public string GameId { get; set; }
    public PlayerInfoModel WhitePlayerInfo { get; set; }
    public PlayerInfoModel BlackPlayerInfo { get; set; }
    public TimeSpan WhiteTimeLeft { get; set; }
    public TimeSpan BlackTimeLeft { get; set; }
    public TimeSpan Increment { get; set; }
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    public ChessGame ChessGame { get; set; } = new();
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();

    public MoveResult MakeMove(string playerId, string from, string to, string? promotion)
    {
        var isWhiteTurn = ChessGame.CurrentPlayer == Player.White;
        var currentPlayerId = isWhiteTurn ? WhitePlayerInfo.Id : BlackPlayerInfo.Id;
        
        if (playerId != currentPlayerId)
            return MoveResult.Fail("It's not your turn");
        
        ReduceTime(isWhiteTurn);
        if ((currentPlayerId == WhitePlayerInfo.Id && WhiteTimeLeft < TimeSpan.Zero) ||
            (currentPlayerId == BlackPlayerInfo.Id && BlackTimeLeft < TimeSpan.Zero))
            return MoveResult.Fail("Time is ran out.");

        var promotionChar = promotion?[0];
        var move = new Move(new(from), new(to), 
            ChessGame.CurrentPlayer, promotionChar);

        if (ChessGame.MakeMove(move, true) is MoveType.Invalid)
            return MoveResult.Fail("Move is invalid.");

        DoIncrement(isWhiteTurn);
        return MoveResult.Ok();
    }

    private void ReduceTime(bool isWhiteTurn)
    {
        var deltaTime = DateTime.UtcNow - LastMoveTime;
        LastMoveTime = DateTime.UtcNow;
        if (isWhiteTurn)
            WhiteTimeLeft -= deltaTime;
        else
            BlackTimeLeft -= deltaTime;
    }

    private void DoIncrement(bool isWhiteTurn)
    {
        if (isWhiteTurn)
            WhiteTimeLeft += Increment;
        else
            BlackTimeLeft += Increment;
    }
}