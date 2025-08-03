namespace CoffeeChess.Application.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string className, string id) : base($"{className} with id {id} not found.") { }
    
    public NotFoundException(string message) : base(message) { }
}