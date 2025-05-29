using System.Collections.Concurrent;
using CoffeeChess.Core;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, GameState> ActiveGames = new();

    public async Task SendMove(Guid gameId, string from, string to, string promotion, string fen)
    {
        if (!ActiveGames.TryGetValue(gameId, out var gameState))
            return;
        gameState.Moves.Add(new MoveRecord
        {
            From = from,
            To = to,
            Promotion = promotion,
            FenAfterMove = fen,
            Timestamp = DateTime.UtcNow,
        });
        gameState.CurrentFen = fen;
        gameState.SwitchTurn();
        gameState.LastMoveTime = DateTime.UtcNow;
        await Clients.Group(gameId.ToString()).SendAsync("RecieveMove", from, to, promotion, fen,
            gameState.PlayerWhiteTimeLeft,
            gameState.PlayerBlackTimeLeft);
    }

    public async Task SendChatMessage(Guid gameId, string user, string message)
    {
        await Clients.Group(gameId.ToString()).SendAsync("RecieveChatMessage", user, message);
    }

    public async Task JoinGame(Guid gameId, string userId) // userId - имя пользователя или его ID
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());

        var newGame = false;
        if (!ActiveGames.TryGetValue(gameId, out var gameState))
        {
            gameState = new GameState(gameId);
            ActiveGames.TryAdd(gameId, gameState);
            newGame = true;
        }

        var playerColor = "s";
        if (string.IsNullOrEmpty(gameState.PlayerWhiteId) && gameState.PlayerBlackId != userId)
        {
            gameState.PlayerWhiteId = userId;
            playerColor = "w";
            if (newGame)
                gameState.CurrentTurnPlayerId = userId;
        }
        else if (string.IsNullOrEmpty(gameState.PlayerBlackId) && gameState.PlayerWhiteId != userId)
        {
            gameState.PlayerBlackId = userId;
            playerColor = "b";
        }
        else if (gameState.PlayerWhiteId == userId) playerColor = "w";
        else if (gameState.PlayerBlackId == userId) playerColor = "b";

        await Clients.Client(Context.ConnectionId).SendAsync("InitialGameState",
            gameState.CurrentFen,
            gameState.Moves,
            playerColor,
            gameState.PlayerWhiteId,
            gameState.PlayerBlackId,
            gameState.PlayerWhiteTimeLeft,
            gameState.PlayerBlackTimeLeft,
            gameState.CurrentTurnPlayerId);


        await Clients.Group(gameId.ToString()).SendAsync("PlayerJoined", userId, playerColor, gameState.PlayerWhiteId,
            gameState.PlayerBlackId);

        if (!string.IsNullOrEmpty(gameState.PlayerWhiteId) && !string.IsNullOrEmpty(gameState.PlayerBlackId) &&
            gameState.IsWhiteTurn && gameState.Moves.Count == 0)
        {
            StartTimerLoop(gameId);
        }
    }

    public async Task RequestResign(Guid gameId, string userId)
    {
        await Clients.Group(gameId.ToString()).SendAsync("PlayerResigned", userId);

        if (ActiveGames.TryGetValue(gameId, out var gameState))
        {
            gameState.StopTimer();
        }
    }

    public async Task OfferDraw(Guid gameId, string userId)
    {
        await Clients.OthersInGroup(gameId.ToString()).SendAsync("DrawOffered", userId);
    }

    public async Task RespondToDraw(Guid gameId, string userId, bool accepted)
    {
        await Clients.Group(gameId.ToString()).SendAsync("DrawResponse", userId, accepted);
        if (accepted)
        {
            if (ActiveGames.TryGetValue(gameId, out var gameState))
            {
                gameState.StopTimer();
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private void StartTimerLoop(Guid gameId)
    {
        if (!ActiveGames.TryGetValue(gameId, out var gameState) || gameState.TimerTokenSource != null)
        {
            return;
        }

        gameState.TimerTokenSource = new CancellationTokenSource();

        var token = gameState.TimerTokenSource.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000, token).ContinueWith(t => { }, token);
                if (token.IsCancellationRequested) break;

                var changed = false;
                if (ActiveGames.TryGetValue(gameId, out var currentGameState) && !currentGameState.IsGameOver)
                {
                    var elapsedSinceLastTick = TimeSpan.FromSeconds(1);

                    if (currentGameState.CurrentTurnPlayerId == currentGameState.PlayerWhiteId)
                    {
                        currentGameState.PlayerWhiteTimeLeft -= elapsedSinceLastTick;
                        if (currentGameState.PlayerWhiteTimeLeft.TotalSeconds <= 0)
                        {
                            currentGameState.PlayerWhiteTimeLeft = TimeSpan.Zero;
                            currentGameState.IsGameOver = true;
                        }
                    }
                    else if (currentGameState.CurrentTurnPlayerId == currentGameState.PlayerBlackId)
                    {
                        currentGameState.PlayerBlackTimeLeft -= elapsedSinceLastTick;
                        if (currentGameState.PlayerBlackTimeLeft.TotalSeconds <= 0)
                        {
                            currentGameState.PlayerBlackTimeLeft = TimeSpan.Zero;
                            currentGameState.IsGameOver = true;
                        }
                    }

                    changed = true;
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    break;
                }

                if (!changed || !ActiveGames.TryGetValue(gameId, out var updatedGameState)) continue;
                await Clients.Group(gameId.ToString()).SendAsync("TimerUpdate",
                    updatedGameState.PlayerWhiteTimeLeft.TotalSeconds,
                    updatedGameState.PlayerBlackTimeLeft.TotalSeconds,
                    updatedGameState.IsGameOver, cancellationToken: token);
                if (updatedGameState.IsGameOver)
                    break;
            }
        }, token);
    }
}