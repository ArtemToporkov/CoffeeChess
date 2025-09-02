namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly record struct ChessSquare
{
    public int Row { get; }
    public char Column { get; }
    
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
        Column = column;
        if (!(int.TryParse(squareValue[1].ToString(), out var row) && row is >= 1 and <= 8))
            throw new ArgumentException(
                $"Chess square notation \"{squareValue}\" should have a number 1-8 at second position.");
        Row = row;
    }

    public static implicit operator string(ChessSquare squareValue) => $"{squareValue.Column}{squareValue.Row}";

    public override string ToString() => $"{Column}{Row}";
}