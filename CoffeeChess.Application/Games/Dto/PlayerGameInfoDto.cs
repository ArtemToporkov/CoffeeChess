namespace CoffeeChess.Application.Games.Dto;

public record struct PlayerGameInfoDto(
    string GameId,
    bool IsWhite,
    PlayerInfoDto WhitePlayerInfo,
    PlayerInfoDto BlackPlayerInfo,
    List<string> SanMovesHistory,
    List<(string Sender, string Message)> MessagesHistory);