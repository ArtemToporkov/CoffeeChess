namespace CoffeeChess.Domain.Games.ValueObjects;

public readonly struct SanMove
{
    private readonly string _value;
    
    public SanMove(string sanMoveValue)
    {
        // TODO: add regex validation
        if (string.IsNullOrEmpty(sanMoveValue))
            throw new ArgumentNullException(nameof(sanMoveValue));
        _value = sanMoveValue;
    }

    public static implicit operator string(SanMove move) => move._value;
}