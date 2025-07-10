using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;

namespace CoffeeChess.Web.Hubs;

public interface IGameClient
{
    public Task GameStarted(string gameId, bool isWhite, 
        PlayerInfoModel whitePlayerInfo, PlayerInfoModel blackPlayerInfo, double totalMillisecondsForOnePlayerLeft);

    public Task ReceiveChatMessage(string username, string message);

    public Task CriticalError(string message);

    public Task MakeMove(string pgn, double whiteMillisecondsLeft, double blackMillisecondsLeft);

    public Task MoveFailed(string message);

    public Task PerformingGameActionFailed(string message);

    public Task PerformGameAction(GameActionPayloadModel payload);

    public Task UpdateGameResult(GameResultPayloadModel payload);
}