import { GameManager } from "./GameManager.js";

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
    
    const whitePlayerInfo = JSON.parse(localStorage.getItem('whitePlayerInfo'));
    const blackPlayerInfo = JSON.parse(localStorage.getItem('blackPlayerInfo'));
    
    $('#whiteUsername').text(whitePlayerInfo.name);
    $('#blackUsername').text(blackPlayerInfo.name);
    $('#whiteRating').text(whitePlayerInfo.rating);
    $('#blackRating').text(blackPlayerInfo.rating);
    
    if (!gameManager.isWhite) {
        gameManager.board.flip();
        $('.game-middle-panel').addClass('flipped');
    }
    
    connection.on("MakeMove", (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        gameManager.updateGameState(pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft);
    });
    
    connection.on("MoveFailed", (errorMessage) => {
        // TODO: undo move, display error message
    });
    
    connection.on("CriticalError", (errorMessage) => {
        // TODO: raise 500 with errorMessage
    });
    
    connection.start();
});