namespace CoffeeChess.Domain.ValueObjects;

public struct ChatMessage(string Username, string message)
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}