using ChessDotNetCore;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;

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

        if (moveKind is MoveType.Invalid)
            return new MoveResult { Valid = false };

        var isCaptureOrPawnMove = game.LastMove!.CapturedPiece is not null ||
                                  game.LastMove!.Piece.GetFenCharacter() is 'p' or 'P';
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
            IsCaptureOrPawnMove = isCaptureOrPawnMove,
            MoveResultType = moveResultType,
        };
    }

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