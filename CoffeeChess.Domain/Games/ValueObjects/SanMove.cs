using System.Text.RegularExpressions;

namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly partial record struct SanMove
{
    private readonly string _value;
    private static readonly Regex SanRegex = RegexForSan();
    
    public SanMove(string sanMoveValue)
    {
        if (string.IsNullOrEmpty(sanMoveValue))
            throw new ArgumentException("Move in SAN notation can't be empty.");
        
        if (!SanRegex.IsMatch(sanMoveValue))
            throw new ArgumentException($"Move in SAN notation \"{sanMoveValue}\" can't be matched.");
        
        _value = sanMoveValue;
    }

    public static implicit operator string(SanMove move) => move._value;
    
    public override string ToString() => _value;

    [GeneratedRegex(@"^([NBRQK]?[a-h]?[1-8]?[x-]?[a-h][1-8](=[NBRQ]| ?e\.p\.)?|^O-O(?:-O)?)[+#$]?$")]
    private static partial Regex RegexForSan();
}