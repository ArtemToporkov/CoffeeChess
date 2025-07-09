import { GameHubMethods } from "./enums/GameHubMethods.js";

$(document).ready(() => {
    
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    connection.on(GameHubMethods.GameStarted, (gameId, isWhite, whitePlayerInfo,
                                               blackPlayerInfo, totalMillisecondsLeft) => {
        localStorage.setItem("totalMillisecondsLeft", totalMillisecondsLeft);
        localStorage.setItem("isWhite", isWhite);
        localStorage.setItem("whitePlayerInfo", JSON.stringify(whitePlayerInfo));
        localStorage.setItem("blackPlayerInfo", JSON.stringify(blackPlayerInfo));
        window.location.href = `/Game/Play/${gameId}`;
    });

    const gameSettings = $("#gameSettings").data('gameSettings');
    console.log(gameSettings);
    
    async function startSignalR() {
        try {
            await connection.start();
            await connection.invoke(GameHubMethods.CreateOrJoinGame, gameSettings);
        } catch (err) {
            console.error(err.toString());
        }
    }
    
    startSignalR();
});