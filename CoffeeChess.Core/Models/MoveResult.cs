namespace CoffeeChess.Core.Models;

public class MoveResult(bool success, string message)
{
    public bool Success { get; } = success;
    public string Message { get; } = message;

    public static MoveResult Ok() => new(true, string.Empty);

    public static MoveResult Fail(string message) => new(false, message);
}