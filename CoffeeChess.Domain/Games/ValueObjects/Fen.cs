namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly struct Fen
{
    private readonly string _value;
    
    public Fen(string fenValue)
    {
        // TODO: add regex validation
        if (string.IsNullOrEmpty(fenValue))
            throw new ArgumentNullException(nameof(fenValue));
        _value = fenValue;
    }

    public static implicit operator string(Fen fen) => fen._value;
}