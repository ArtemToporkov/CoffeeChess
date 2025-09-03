namespace CoffeeChess.Application.Games.Dto;

public record struct PlayerGameInfoDto(
    string GameId,
    bool IsWhite,
    PlayerInfoDto WhitePlayerInfoDto,
    PlayerInfoDto BlackPlayerInfoDto,
    List<string> SanMovesHistory,
    List<(string Sender, string Message)> MessagesHistory);