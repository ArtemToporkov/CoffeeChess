namespace CoffeeChess.Core;

public class MoveRecord
{
    public string From { get; set; }
    public string To { get; set; }
    public string? Promotion { get; set; }
    public string FenAfterMove { get; set; }
    public DateTime Timestamp { get; set; }
    public string Piece { get; set; }
    public string San { get; set; }
}