﻿using ChessDotNetCore;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using MoveKind = ChessDotNetCore.MoveType;
using MoveType = CoffeeChess.Domain.Games.Enums.MoveType;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class ChessDotNetCoreMovesValidatorService : IChessMovesValidator
{
    public MoveResult ApplyMove(Fen currentFen, PlayerColor playerColor, string from, string to, char? promotion)
    {
        var game = new ChessGame(currentFen);
        var player = playerColor == PlayerColor.White ? Player.White : Player.Black;
        var move = new Move(from, to, player, promotion);
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
                $"[{nameof(ChessDotNetCoreMovesValidatorService)}.{nameof(ApplyMove)}] " +
                $"parsing move type failed: {moveKind}]")
        };
}