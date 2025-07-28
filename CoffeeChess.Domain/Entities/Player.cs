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
        
    public string Id { get; }
    public string Name { get; }
    public int Rating { get; }
}