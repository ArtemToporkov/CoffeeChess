using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class ChessLibMovesValidatorService : IChessMovesValidatorService
{
    public MoveResult ApplyMove(Fen currentFen, PlayerColor playerColor, ChessSquare from, ChessSquare to, Promotion? promotion)
    {
        var game = GameFactory.Create(currentFen);
        var promotionPieceType = ConvertPromotionOrThrow(promotion);
        var fromSquare = ConvertSquareOrThrow(from);
        var toSquare = ConvertSquareOrThrow(to);
        var validMoves = game.Pos.GenerateMoves();
        
        var moveExt = validMoves
            .FirstOrDefault(m => m.Move.FromSquare() == fromSquare 
                                 && m.Move.ToSquare() == toSquare
                                 && (m.Move.MoveType() != MoveTypes.Promotion 
                                     || m.Move.PromotedPieceType() == promotionPieceType));
        if (moveExt == ExtMove.Empty || !moveExt.Move.IsValidMove())
            return MoveResult.Fail;
        var move = moveExt.Move;
        
        var movedPiece = game.Pos.MovedPiece(move);
        var isCaptureOrPawnMove = game.Pos.IsCapture(move)
                                  || movedPiece == Pieces.BlackPawn
                                  || movedPiece == Pieces.WhitePawn;
        var sanNotation = new SanNotation(game.Pos);
        var san = sanNotation.Convert(move);
        if (move.MoveType() == MoveTypes.Enpassant)
            san = ConvertEnPassantSanNotation(san);
        game.Pos.MakeMove(move, new State());
        return new MoveResult
        {
            Valid = true,
            FenAfterMove = new Fen(game.Pos.FenNotation),
            IsCaptureOrPawnMove = isCaptureOrPawnMove,
            San = new San(san)
        };
    }

    private static PieceTypes ConvertPromotionOrThrow(Promotion? promotion)
    {
        var promotionPieceType = promotion switch
        {
            null => PieceTypes.NoPieceType,
            Promotion.Queen => PieceTypes.Queen,
            Promotion.Knight => PieceTypes.Knight,
            Promotion.Bishop => PieceTypes.Bishop,
            Promotion.Rook => PieceTypes.Rook,
            _ => throw new ArgumentOutOfRangeException(nameof(promotion), 
                $"Unexpected argument for \"{nameof(Promotion)}\".")
        };
        return promotionPieceType;
    }

    private static Square ConvertSquareOrThrow(ChessSquare square)
    {
        var rank = square.Row switch
        {
            1 => Rank.Rank1,
            2 => Rank.Rank2,
            3 => Rank.Rank3,
            4 => Rank.Rank4,
            5 => Rank.Rank5,
            6 => Rank.Rank6,
            7 => Rank.Rank7,
            8 => Rank.Rank8,
            _ => throw new ArgumentOutOfRangeException(nameof(square), 
                $"Unexpected square row \"{nameof(ChessSquare.Row)}\".")
        };
        var file = square.Column switch
        {
            'a' => File.FileA,
            'b' => File.FileB,
            'c' => File.FileC,
            'd' => File.FileD,
            'e' => File.FileE,
            'f' => File.FileF,
            'g' => File.FileG,
            'h' => File.FileH,
            _ => throw new ArgumentOutOfRangeException(nameof(square), 
                $"Unexpected square column \"{nameof(ChessSquare.Column)}\"")
        };
        var convertedSquare = new Square((rank, file));
        return convertedSquare;
    }

    private static string ConvertEnPassantSanNotation(string notation)
    {
        // NOTE: It seems that Rudzoft.ChessLib.Notation.Notations.SanNotation converts an en passant move
        // to something like "epgf6", i guess "ep" means "en passant", "gf6" means
        // "a pawn from the g file takes a pawn from the f6 square. So it should be "gxf6".
        var result = new[]
        {
            notation[2],
            'x',
            notation[3],
            notation[4]
        };
        return new string(result);
    }
}