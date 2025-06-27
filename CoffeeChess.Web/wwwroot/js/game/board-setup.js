import { GameManager } from "./GameManager.js";
import {ChatManager} from "./ChatManager.js";
import {loadUi} from "./ui.js";

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
    loadUi(gameManager);
    
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
    
    connection.start();
});