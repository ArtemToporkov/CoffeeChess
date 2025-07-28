namespace CoffeeChess.Domain.Entities;

public class Player(string id, string name, int rating)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public int Rating { get; } = rating;
}