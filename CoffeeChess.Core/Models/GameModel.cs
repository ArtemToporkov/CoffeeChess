using System.Collections.Concurrent;
using System.Text;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models;

public class GameModel
{
    public string GameId { get; set; }
    public PlayerInfoModel WhitePlayerInfo { get; set; }
    public PlayerInfoModel BlackPlayerInfo { get; set; }
    public bool IsOver => ChessGame.GameResult != GameResult.OnGoing;
    public TimeSpan WhiteTimeLeft { get; set; }
    public TimeSpan BlackTimeLeft { get; set; }
    public TimeSpan Increment { get; set; }
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    private ChessGame ChessGame { get; set; } = new();
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();

    public MoveResult MakeMove(string playerId, string from, string to, string? promotion)
    {
        var isWhiteTurn = ChessGame.CurrentPlayer == Player.White;
        var currentPlayerId = isWhiteTurn ? WhitePlayerInfo.Id : BlackPlayerInfo.Id;
        
        if (playerId != currentPlayerId)
            return MoveResult.NotYourTurn;
        
        ReduceTime(isWhiteTurn);
        if ((currentPlayerId == WhitePlayerInfo.Id && WhiteTimeLeft < TimeSpan.Zero) ||
            (currentPlayerId == BlackPlayerInfo.Id && BlackTimeLeft < TimeSpan.Zero))
            return MoveResult.TimeRanOut;

        var promotionChar = promotion?[0];

        var move = new Move(from, to, isWhiteTurn ? Player.White : Player.Black, promotionChar);
        if (ChessGame.MakeMove(move, false) is MoveType.Invalid)
            return MoveResult.Invalid;

        if (ChessGame.ThreeFoldRepeatAndThisCanResultInDraw)
            return MoveResult.ThreeFold;

        if (ChessGame.FiftyMovesAndThisCanResultInDraw)
            return MoveResult.FiftyMovesRule;
        
        if (ChessGame.IsCheckmated(Player.White) || ChessGame.IsCheckmated(Player.Black))
            return MoveResult.Checkmate;
        
        if (ChessGame.IsStalemated(Player.White) || ChessGame.IsStalemated(Player.Black))
            return MoveResult.Stalemate;
        
        DoIncrement(isWhiteTurn);
        return MoveResult.Success;
    }

    public string GetPgn()
    {
        var pgnBuilder = new StringBuilder();
        for (var i = 0; i < ChessGame.Moves.Count; i++)
        {
            if (i % 2 == 0)
                pgnBuilder.Append($"{i / 2 + 1}. {ChessGame.Moves[i].SAN} ");
            else
                pgnBuilder.Append($"{ChessGame.Moves[i].SAN} ");
        }

        return pgnBuilder.ToString().Trim();
    }

    public void ClaimDraw() => ChessGame.ClaimDraw();

    public void Resign(PlayerColor player) => ChessGame.Resign(player == PlayerColor.White 
        ? Player.White : Player.Black);

    public (PlayerInfoModel? Winner, PlayerInfoModel? Loser) GetWinnerAndLoser()
    {
        if (!ChessGame.IsWinner(Player.White) && !ChessGame.IsWinner(Player.Black))
        {
            return (null, null);
        }
        var (winner, loser) = ChessGame.IsWinner(Player.White) 
            ? (WhitePlayerInfo, BlackPlayerInfo)
            : (BlackPlayerInfo, WhitePlayerInfo);
        return (winner, loser);
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