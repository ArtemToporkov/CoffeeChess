namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly record struct ChessSquare
{
    private readonly int _row;
    private readonly char _column;
    
    public ChessSquare(string squareValue)
    {
        if (string.IsNullOrEmpty(squareValue))
            throw new ArgumentNullException(nameof(squareValue));

        if (squareValue.Length != 2)
            throw new ArgumentException("Chess square notation should consist of 2 characters: column and row.");
        
        var column = squareValue[0];
        if (column is < 'a' or > 'h')
            throw new ArgumentException(
                $"Chess square notation \"{squareValue}\" " +
                $"should have a character a, b, c, d, e, f, g or h at first position.");
        _column = squareValue[1];
        if (!(int.TryParse(squareValue, out _row) && _row is >= 1 and <= 8))
            throw new ArgumentException(
                $"Chess square notation \"{squareValue}\" should have a number 1-8 at second position.");
    }

    public static implicit operator string(ChessSquare squareValue) => $"{squareValue._column}{squareValue._row}";
}