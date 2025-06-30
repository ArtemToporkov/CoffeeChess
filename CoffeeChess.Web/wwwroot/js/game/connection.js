import { GameManager } from "./managers/GameManager.js";
import { ChatManager } from "./managers/ChatManager.js";
import { loadUi, receiveDrawOffer, turnButtonsBack, updateGameResult, turnOffDrawResignInfo } from "./ui.js";
import { GameActionType } from "./GameActionType.js";

$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    
    const gameManager = new GameManager(
        connection, 
        gameId, 
        localStorage.getItem('isWhite') === "true",
        localStorage.getItem('totalMillisecondsLeft')
    );
    const chatManager = new ChatManager(connection, gameId);
    loadUi(connection, gameManager, gameId);
    
    connection.on("MakeMove", (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        gameManager.updateGameState(pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft);
    });
    
    connection.on("MoveFailed", (errorMessage) => {
        // TODO: undo move, display error message
    });
    
    connection.on("CriticalError", (errorMessage) => {
        // TODO: raise 500 with errorMessage
    });

    connection.on("ReceiveChatMessage", (user, message) => {
        chatManager.addMessageToChat(user, message);
    });
    
    connection.on("PerformGameAction", payload => {
        switch (payload.gameActionType) {
            case GameActionType.ReceiveDrawOffer:
                receiveDrawOffer(connection, payload.message);
                break;
            case GameActionType.GetDrawOfferDeclination:
                turnButtonsBack();
                break;
        }
    });
    
    connection.on("UpdateGameResult", payload => {
        gameManager.endGame();
        turnOffDrawResignInfo();
        updateGameResult(payload.result, payload.message, payload.oldRating, payload.newRating);
    });
    
    connection.start();
});