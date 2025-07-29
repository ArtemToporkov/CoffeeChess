import { GameManager } from "./managers/GameManager.js";
import { ChatManager } from "./managers/ChatManager.js";
import { loadUi, 
    receiveDrawOffer, 
    turnButtonsBack, 
    updateGameResult, 
    turnOffDrawResignInfo, 
    setDrawOfferInactive, 
    setDrawOfferActive, 
    playRatingsChangeAnimation } from "./ui.js";
import { GameActionType } from "./enums/GameActionType.js";
import { GameHubMethods } from "./enums/GameHubMethods.js";

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
    
    connection.on(GameHubMethods.MakeMove, (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        gameManager.updateGameState(pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft);
    });
    
    connection.on(GameHubMethods.MoveFailed, (errorMessage) => {
        gameManager.undoLastMove();
        chatManager.addMessageToChat("CoffeeChess", errorMessage);
    });
    
    connection.on(GameHubMethods.CriticalError, (errorMessage) => {
        console.error(errorMessage);
    });

    connection.on(GameHubMethods.ReceiveChatMessage, (user, message) => {
        chatManager.addMessageToChat(user, message);
    });
    
    connection.on(GameHubMethods.PerformGameAction, payload => {
        switch (payload.gameActionType) {
            case GameActionType.SendDrawOffer:
                setDrawOfferInactive();
                break;
            case GameActionType.ReceiveDrawOffer:
                receiveDrawOffer(connection, payload.message);
                break;
            case GameActionType.DeclineDrawOffer:
                turnButtonsBack();
                break;
            case GameActionType.GetDrawOfferDeclination:
                setDrawOfferActive();
                break;
        }
    });
    
    connection.on(GameHubMethods.PerformingGameActionFailed, (errorMessage) => {
       // TODO: tell client that performing game action is impossible 
    });
    
    connection.on(GameHubMethods.UpdateGameResult, (result, reason) => {
        gameManager.endGame();
        turnOffDrawResignInfo();
        updateGameResult(gameManager.isWhite, result, reason);
    });
    
    connection.on(GameHubMethods.UpdatePlayerRating, (oldRating, newRating) => {
        playRatingsChangeAnimation(oldRating, newRating);
    });
    
    connection.start();
});