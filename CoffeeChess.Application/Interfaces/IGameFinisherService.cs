using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;

namespace CoffeeChess.Application.Interfaces;

public interface IGameFinisherService
{
    public Task SendWinResultAndSave(PlayerInfo winner, PlayerInfo loser, 
        string winReason, string loseReason);

    public Task SendDrawResultAndSave(PlayerInfo first, PlayerInfo second, string reason);
}