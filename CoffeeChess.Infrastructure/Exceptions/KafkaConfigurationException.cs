namespace CoffeeChess.Infrastructure.Exceptions;

public class KafkaConfigurationException(string message) : Exception(message);