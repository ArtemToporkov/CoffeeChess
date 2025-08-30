namespace CoffeeChess.Infrastructure.Exceptions;

public class UnsupportedDomainEventException(string domainModelClassName, string eventName) 
    : Exception($"A domain event \"{eventName}\" for a model " +
                $"of class \"{domainModelClassName}\" is not supported.");