namespace CoffeeChess.Web.Exceptions;

public class UserNotFoundException(string userId) : Exception($"User with ID {userId} not found.");