namespace CoffeeChess.Application.Games.Dto;

public record struct PlayerInfoDto(string Name, int Rating, double MillisecondsLeft);