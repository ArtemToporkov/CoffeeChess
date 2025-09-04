namespace CoffeeChess.Application.Games.Dto;

public record struct PlayerGameInfoDto(
    string GameId,
    bool IsWhite,
    PlayerInfoDto WhitePlayerInfo,
    PlayerInfoDto BlackPlayerInfo,
    string Pgn,
    List<(string Sender, string Message)> MessagesHistory);