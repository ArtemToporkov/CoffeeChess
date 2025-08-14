import { GameManager } from "./managers/GameManager.js";
import { ChatManager } from "./managers/ChatManager.js";
import { loadUi, 
    receiveDrawOffer, 
    turnButtonsBack, 
    showResultModal, 
    turnOffDrawResignInfo, 
    setDrawOfferInactive, 
    setDrawOfferActive, 
    playRatingsChangeAnimation,
    setResultPoints } from "./ui.js";
import { GameActionType } from "./enums/GameActionType.js";
import { GameHubEvents } from "./enums/GameHubEvents.js";

let connection;
let gameManager;
let chatManager;

const init = async () => {
    await loadScript("/lib/chess.js/chess.min.js");
    await loadScript("/lib/chessboardjs/chessboard-1.0.0.js");
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    
    gameManager = new GameManager(
        connection, 
        gameId, 
        localStorage.getItem('isWhite') === "true",
        localStorage.getItem('totalMillisecondsLeft')
    );
    chatManager = new ChatManager(connection, gameId);
    loadUi(connection, gameManager, gameId);
    
    connection.on(GameHubEvents.MoveMade, (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        gameManager.updateGameState(pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft);
    });
    
    connection.on(GameHubEvents.MoveFailed, (errorMessage) => {
        gameManager.undoLastMove();
        chatManager.addMessageToChat("CoffeeChess", errorMessage);
    });

    connection.on(GameHubEvents.ChatMessageReceived, (user, message) => {
        chatManager.addMessageToChat(user, message);
    });
    
    connection.on(GameHubEvents.GameActionPerformed, payload => {
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
    
    connection.on(GameHubEvents.PerformingGameActionFailed, (errorMessage) => {
       // TODO: tell client that performing game action is impossible 
    });
    
    connection.on(GameHubEvents.GameEnded, (result, reason) => {
        gameManager.endGame();
        turnOffDrawResignInfo();
        setResultPoints(result);
        showResultModal(gameManager.isWhite, result, reason);
    });
    
    connection.on(GameHubEvents.PlayerRatingUpdated, (oldRating, newRating) => {
        playRatingsChangeAnimation(oldRating, newRating);
    });
    
    connection.start();
};

const destroy = () => {
    connection.stop();
    connection = null;
    gameManager = null;
    chatManager = null;
}

export default { init, destroy };