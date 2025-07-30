import { GameHubEvents } from "./enums/GameHubEvents.js";
import { GameHubMethods } from "./enums/GameHubMethods.js";
import { animateSearching } from "./ui.js";

$(async () => {
    await document.fonts.ready;
    animateSearching();
    
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    connection.on(GameHubEvents.GameStarted, (gameId, isWhite, whitePlayerInfo,
                                              blackPlayerInfo, totalMillisecondsLeft) => {
        localStorage.setItem("totalMillisecondsLeft", totalMillisecondsLeft);
        localStorage.setItem("isWhite", isWhite);
        localStorage.setItem("whitePlayerInfo", JSON.stringify(whitePlayerInfo));
        localStorage.setItem("blackPlayerInfo", JSON.stringify(blackPlayerInfo));
        window.location.href = `/Game/Play/${gameId}`;
    });

    const gameSettings = $("#gameSettings").data('gameSettings');
    console.log(gameSettings);

    try {
        await connection.start();
        await connection.invoke(GameHubMethods.QueueChallenge, gameSettings);
    } catch (err) {
        console.error(err.toString());
    }
});