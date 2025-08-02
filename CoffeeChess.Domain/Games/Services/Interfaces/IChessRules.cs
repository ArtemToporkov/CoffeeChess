using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Games.Services.Interfaces;

public interface IChessRules
{
    public MoveResult ApplyMove(string currentFen, PlayerColor playerColor, string from, string to, char? promotion);
}