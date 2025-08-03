namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly struct ChessSquare
{
    private readonly int _row;
    private readonly char _column;
    
    public ChessSquare(string squareValue)
    {
        // TODO: check for a-h
        _column = squareValue[1];
        if (int.TryParse(squareValue, out _row))
            throw new ArgumentException("Chess square notation should have a number at second position.");
    }

    public static implicit operator string(ChessSquare squareValue) => $"{squareValue._column}{squareValue._row}";
}