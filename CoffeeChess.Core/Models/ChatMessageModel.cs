namespace CoffeeChess.Core.Models;

public class ChatMessageModel
{
    public string Username { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}