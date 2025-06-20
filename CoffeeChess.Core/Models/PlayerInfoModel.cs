namespace CoffeeChess.Core.Models;

public class PlayerInfoModel(string id, string name, int rating)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Rating { get; set; } = rating;
}