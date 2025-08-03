using ChessDotNetCore;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using MoveKind = ChessDotNetCore.MoveType;
using MoveType = CoffeeChess.Domain.Games.Enums.MoveType;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class ChessDotNetCoreMovesValidatorService : IChessMovesValidator
{
    public MoveResult ApplyMove(Fen currentFen, PlayerColor playerColor, ChessSquare from, 
        ChessSquare to, Promotion? promotion)
    {
        var game = new ChessGame(currentFen);
        var player = playerColor == PlayerColor.White ? Player.White : Player.Black;
        var promotionChar = ConvertPromotionToChar(promotion);
        
        var move = new Move(from, to, player, promotionChar);
        var moveKind = game.MakeMove(move, false);

        if (moveKind is MoveKind.Invalid)
            return new MoveResult { Valid = false };

        var moveType = ParseMoveKind(moveKind);
        var san = new SanMove(game.LastMove!.SAN);
        var fenAfterMove = new Fen(game.GetFen());
        var moveResultType = MoveResultType.None;
        if (game.IsStalemated(Player.White) || game.IsStalemated(Player.Black))
            moveResultType = MoveResultType.Stalemate;
        else if (game.IsCheckmated(Player.White) || game.IsCheckmated(Player.Black))
            moveResultType = MoveResultType.Checkmate;

        return new MoveResult
        {
            Valid = true,
            San = san,
            FenAfterMove = fenAfterMove,
            MoveType = moveType,
            MoveResultType = moveResultType,
        };
    }

    private static MoveType ParseMoveKind(MoveKind moveKind)
        => moveKind switch
        {
            MoveKind.Capture => MoveType.Capture,
            MoveKind.Move => MoveType.Move,
            MoveKind.Castling => MoveType.Castling,
            MoveKind.Promotion => MoveType.Promotion,
            MoveKind.EnPassant => MoveType.EnPassant,
            _ => throw new ArgumentOutOfRangeException(
                nameof(moveKind), moveKind, 
                "Unexpected ChessDotNetCore.MoveType argument to parse to CoffeeChess.MoveType.")
        };

    private static char? ConvertPromotionToChar(Promotion? promotion)
     => promotion switch
     {
         Promotion.Knight => 'k',
         Promotion.Bishop => 'b',
         Promotion.Rook => 'r',
         Promotion.Queen => 'q',
         null => null,
         _ => throw new ArgumentOutOfRangeException(
             nameof(promotion), promotion, "Unexpected value for enum Promotion.")
     };
}