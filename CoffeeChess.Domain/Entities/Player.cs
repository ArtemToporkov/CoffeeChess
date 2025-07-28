namespace CoffeeChess.Domain.Entities;

public class Player
{
    private Player() { }
    
    public Player(string id, string name, int rating)
    {
        Id = id;
        Name = name;
        Rating = rating;
    }
        
    public string Id { get; init; }
    public string Name { get; private set; }
    public int Rating { get; private set; }
}