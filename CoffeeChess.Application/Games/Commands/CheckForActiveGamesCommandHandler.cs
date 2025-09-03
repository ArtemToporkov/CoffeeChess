using CoffeeChess.Application.Games.Dto;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public class CheckForActiveGamesCommandHandler(
    IChatRepository chatRepository,
    IGameRepository gameRepository) : IRequestHandler<CheckForActiveGamesCommand, PlayerGameInfoDto?>
{
    public async Task<PlayerGameInfoDto?> Handle(CheckForActiveGamesCommand request, CancellationToken cancellationToken)
    {
        var activeGame = await gameRepository.CheckForActiveGames(request.PlayerId);
        if (activeGame == null)
            return null;

        var chat = await chatRepository.GetByIdAsync(activeGame.GameId, cancellationToken);
        var messagesHistory = chat is null 
            ? [] 
            : chat.Messages.Select(message => (Sender: message.Username, Message: message.Message)).ToList();
        
        var isWhite = activeGame.WhitePlayerId == request.PlayerId;
        var gameInfoDto = new PlayerGameInfoDto(
            activeGame.GameId,
            isWhite,
            activeGame.WhiteTimeLeft.TotalMilliseconds,
            activeGame.BlackTimeLeft.TotalMilliseconds,
            activeGame.MovesHistory.Select(x => x.San.ToString()).ToList(),
            messagesHistory);
        return gameInfoDto;
    }
}