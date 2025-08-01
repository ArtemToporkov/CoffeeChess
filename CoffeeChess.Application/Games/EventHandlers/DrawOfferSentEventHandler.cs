using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class DrawOfferSentEventHandler(
    IPlayerRepository playerRepository,
    IGameEventNotifierService notifier) : INotificationHandler<DrawOfferSent>
{
    public async Task Handle(DrawOfferSent notification, CancellationToken cancellationToken)
    {
        var sender = await playerRepository.GetAsync(notification.SenderId);
        var receiver = await playerRepository.GetAsync(notification.ReceiverId);
        var message = $"{sender!.Name} offers a draw";
        await notifier.NotifyDrawOfferSent(message, sender.Id, receiver!.Id);
    }
}