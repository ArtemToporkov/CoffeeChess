namespace CoffeeChess.Application.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string className, string id) : base($"{className} with ID {id} not found.") { }
    
    public NotFoundException(string message) : base(message) { }
}