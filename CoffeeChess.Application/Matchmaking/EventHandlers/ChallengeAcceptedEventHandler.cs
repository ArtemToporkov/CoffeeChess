using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Events;
using MediatR;

namespace CoffeeChess.Application.Matchmaking.EventHandlers;

public class ChallengeAcceptedEventHandler(
    IGameRepository gameRepository, 
    IChatRepository chatRepository) : INotificationHandler<ChallengeAccepted>
{
    private static readonly Lock Lock = new();
    private static readonly Random Random = new();
    
    public async Task Handle(ChallengeAccepted notification, CancellationToken cancellationToken)
    {
        var acceptedChallengeOwner = ChooseColor(notification.AcceptedChallenge.ColorPreference);
        var (whitePlayerId, blackPlayerId) = acceptedChallengeOwner == ColorPreference.White
            ? (notification.OwnerId, notification.AcceptorId)
            : (notification.AcceptorId, notification.OwnerId);
        var createdGame = new Game(
            Guid.NewGuid().ToString("N")[..8],
            whitePlayerId,
            blackPlayerId,
            TimeSpan.FromMinutes(notification.AcceptedChallenge.TimeControl.Minutes),
            TimeSpan.FromSeconds(notification.AcceptedChallenge.TimeControl.Increment)
        );
        await gameRepository.AddAsync(createdGame, cancellationToken);
        var chat = new Chat(createdGame.GameId);
        await chatRepository.AddAsync(chat, cancellationToken);
        await gameRepository.SaveChangesAsync(createdGame, cancellationToken);
    }
    
    private static ColorPreference ChooseColor(ColorPreference colorPreference)
        => colorPreference switch
        {
            ColorPreference.White => ColorPreference.White,
            ColorPreference.Black => ColorPreference.Black,
            ColorPreference.Any => GetRandomColor(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(colorPreference), colorPreference, "Unexpected color preference.")
        };
    
    private static ColorPreference GetRandomColor()
    {
        lock (Lock)
            return Random.Next(0, 2) == 0
                ? ColorPreference.White
                : ColorPreference.Black;
    }
}