namespace CoffeeChess.Infrastructure.Exceptions;

public class KeyAlreadyExistsException(string key) : Exception($"Key with value \"{key}\" already exists.");