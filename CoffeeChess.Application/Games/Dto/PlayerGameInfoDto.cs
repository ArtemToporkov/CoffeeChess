namespace CoffeeChess.Application.Games.Dto;

public record struct PlayerGameInfoDto(
    string GameId,
    bool IsWhite,
    double CurrentWhiteMilliseconds,
    double CurrentBlackMilliseconds,
    List<string> SanMovesHistory,
    List<(string Sender, string Message)> MessagesHistory);