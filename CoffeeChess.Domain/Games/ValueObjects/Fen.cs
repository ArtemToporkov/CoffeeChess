using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly partial record struct Fen
{
    public string PiecesPlacement { get; }
    public int PliesCount { get; }
    public string Value { get; }
    
    private static readonly Regex FenRegex = RegexForFen();
    
    public Fen(string fenValue)
    {
        if (string.IsNullOrEmpty(fenValue))
            throw new ArgumentException("Position in FEN notation can't be empty.");
        
        if (!FenRegex.IsMatch(fenValue))
            throw new ArgumentException($"Position in FEN notation \"{fenValue}\" can't be matched.");
        
        var fenParts = fenValue.Split(' ');
        PiecesPlacement = fenParts[0];
        if (!int.TryParse(fenParts[4], out var pliesCount))
            throw new ArgumentException(
                $"Position in FEN notation \"{fenValue}\" should contain an info about plies count.");
        PliesCount = pliesCount;
        Value = fenValue;
    }

    public static implicit operator string(Fen fen) => fen.Value;

    public override string ToString() => Value;

    [GeneratedRegex("""
                    ^(?:[PNBRQKpnbrqk1-8]{1,8}(?:\/[PNBRQKpnbrqk1-8]{1,8}){7})\s(?:[wb])\s(?:-
                    |[KQkq]{1,4})\s(?:-|[a-h][36])\s(\d+)\s([1-9]\d*)$
                    """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex RegexForFen();
}