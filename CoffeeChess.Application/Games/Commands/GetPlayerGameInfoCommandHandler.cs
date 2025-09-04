using CoffeeChess.Application.Games.Dto;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public class GetPlayerGameInfoCommandHandler(
    IPgnBuilderService pgnBuilder,
    IPlayerRepository playerRepository,
    IChatRepository chatRepository,
    IGameRepository gameRepository) : IRequestHandler<GetPlayerGameInfoCommand, PlayerGameInfoDto?>
{
    public async Task<PlayerGameInfoDto?> Handle(GetPlayerGameInfoCommand request, CancellationToken cancellationToken)
    {
        var activeGame = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (activeGame == null)
            return null;
        
        var whiteInfoDto = await GetPlayerInfoAsync(activeGame, true, cancellationToken);
        if (whiteInfoDto == null)
            return null;
        var blackInfoDto = await GetPlayerInfoAsync(activeGame, false, cancellationToken);
        if (blackInfoDto == null)
            return null;

        var chat = await chatRepository.GetByIdAsync(activeGame.GameId, cancellationToken);
        if (chat is null)
            return null;
        var messagesHistory = chat.Messages.Select(
            message => (Sender: message.Username, Message: message.Message)).ToList();
        var pgn = pgnBuilder.GetPgnWithMovesOnly(activeGame.MovesHistory.Select(x => x.San).ToList());
        
        var isWhite = activeGame.WhitePlayerId == request.PlayerId;
        var gameInfoDto = new PlayerGameInfoDto(
            activeGame.GameId,
            isWhite,
            whiteInfoDto.Value,
            blackInfoDto.Value,
            pgn,
            messagesHistory);
        return gameInfoDto;
    }

    private async Task<PlayerInfoDto?> GetPlayerInfoAsync(Game game, bool isForWhite, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetByIdAsync(
            isForWhite ? game.WhitePlayerId : game.BlackPlayerId, cancellationToken);
        if (player == null)
            return null;
        
        var playerInfoDto = new PlayerInfoDto(
            player.Name, 
            player.Rating, 
            isForWhite 
                ? game.WhiteTimeLeft.TotalMilliseconds
                : game.BlackTimeLeft.TotalMilliseconds);
        return playerInfoDto;
    }
}