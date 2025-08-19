namespace CoffeeChess.Domain.Matchmaking.ValueObjects;

public readonly struct TimeControl
{
    public int Minutes { get; }
    public int Increment { get; }
    
    public TimeControl(int minutes, int increment)
    {
        if (minutes < 0)
            throw new ArgumentException($"{nameof(minutes)} should be greater than or equal to 0.");
        if (increment < 0)
            throw new ArgumentException($"{nameof(increment)} should be greater than or equal to 0.");
        if (increment > 59)
            throw new ArgumentException($"{nameof(increment)} should be less than or equal to 59.");
        Minutes = minutes;
        Increment = increment;
    }
    
}