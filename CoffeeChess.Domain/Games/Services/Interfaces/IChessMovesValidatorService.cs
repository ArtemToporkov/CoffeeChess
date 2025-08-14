using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Games.Services.Interfaces;

public interface IChessMovesValidatorService
{
    public MoveResult ApplyMove(Fen currentFen, PlayerColor playerColor, 
        ChessSquare from, ChessSquare to, Promotion? promotion);
}