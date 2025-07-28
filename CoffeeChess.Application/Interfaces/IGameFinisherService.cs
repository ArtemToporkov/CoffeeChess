using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;

namespace CoffeeChess.Application.Interfaces;

public interface IGameFinisherService
{
    public Task SendWinResultAndSave(Player winner, Player loser, 
        string winReason, string loseReason);

    public Task SendDrawResultAndSave(Player first, Player second, string reason);
}