import { HistoryManager } from "../game/managers/HistoryManager.js";
import { GameResult } from "../game/enums/GameResult.js";

($(document).ready(async () => {
    const pathParts = window.location.pathname.split('/');
    // const gameId = pathParts[pathParts.length - 1];
    const gameId = "7ed025c3";
    console.log(gameId);
    const game = await $.ajax({
        url: `/GamesHistory/GetGame/${gameId}`,
        dataType: 'json'
    });
    const username = $('#username').text();
    if (!username) {
        document.title = `${game.whitePlayerName} vs. ${game.blackPlayerName} - CoffeeChess`;
    } else if (username === game.whitePlayerName) {
        document.title = `Review vs. ${game.blackPlayerName} - CoffeeChess`
    } else {
        document.title = `Review vs. ${game.whitePlayerName} - CoffeeChess`
    }
    setUiForGame(game);
}));

function setUiForGame(game) {
    const chess = new Chess();
    const board = new ChessBoard('reviewBoard', getConfig());
    const historyManager = new HistoryManager(
        'reviewBoard', fen => board.position(fen)
    );
    
    for (let el of game.sanMovesHistory) {
        chess.move(el);
        const currentFen = chess.fen();
        const move = { san: el };
        historyManager.update(move, currentFen);
    }
    
    board.position(chess.fen());
    setNamesAndRatings(game);
    setResult(game);
}

function setNamesAndRatings(game) {
    $('#whiteUsername').text(game.whitePlayerName);
    $('#whiteRating').text(game.whitePlayerRating);
    $('#blackUsername').text(game.blackPlayerName);
    $('#blackRating').text(game.blackPlayerRating);
}

function setResult(game) {
    const $whitePoints = $('#whitePoints');
    const $blackPoints = $('#blackPoints');
    switch (game.gameResult) {
        case GameResult.WhiteWon:
            $whitePoints.text('1');
            $blackPoints.text('0');
            break;
        case GameResult.Draw:
            $whitePoints.text('½');
            $blackPoints.text('½');
            break;
        case GameResult.BlackWon:
            $whitePoints.text('0');
            $blackPoints.text('1');
            break;
    }
}

function getConfig() {
    return {
        draggable: false,
        position: 'start',
        pieceTheme: '/img/chesspieces/{piece}.png',
        snapbackSpeed: 250
    };
}