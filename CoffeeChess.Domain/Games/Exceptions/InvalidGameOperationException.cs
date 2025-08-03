namespace CoffeeChess.Domain.Games.Exceptions;

public class InvalidGameOperationException(string message) : Exception(message);