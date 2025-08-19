namespace CoffeeChess.Domain.Matchmaking.ValueObjects;

public readonly struct EloRatingPreference
{
    public const int MinValue = 0;
    public const int MaxValue = int.MaxValue;

    public int Min { get; }
    public int Max { get; }

    public EloRatingPreference(int min, int max)
    {
        if (min < MinValue)
            throw new ArgumentException($"{nameof(min)} should be greater than {MinValue}.");
        if (max < MinValue)
            throw new ArgumentException($"{nameof(max)} should be greater than {MinValue}.");
        if (min > max)
            throw new ArgumentException($"{nameof(min)} should be less than or equal to {max}.");
        Min = min;
        Max = max;
    }

    public static EloRatingPreference Any => new(MinValue, MaxValue);
}