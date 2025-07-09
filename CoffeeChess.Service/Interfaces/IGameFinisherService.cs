using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameFinisherService
{
    public Task SendWinResultAndSave(PlayerInfoModel winner, PlayerInfoModel loser, 
        string winReason, string loseReason);

    public Task SendDrawResultAndSave(PlayerInfoModel first, PlayerInfoModel second, string reason);
}