import { HistoryManager } from "../managers/HistoryManager.js";
import { GameResult } from "../enums/GameResult.js";
import { GameRole } from "../enums/GameRole.js"
import { GameResultReason } from "../enums/GameResultReason.js";
import { closeResultPanel, playRatingsChangeAnimation, setTimerHighlighting } from "../ui.js";

let chess;
let board;
let historyManager;

const init = async () => {
    await loadScript("/lib/chess.js/chess.min.js");
    await loadScript("/lib/chessboardjs/chessboard-1.0.0.js");
    
    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    const game = await $.ajax({
        url: `/GamesHistory/GetGame/${gameId}`,
        dataType: 'json'
    });
    const username = $('#username').text();
    let gameRole;
    if (!username) {
        gameRole = GameRole.Spectator;
    } else if (username === game.whitePlayerName) {
        gameRole = GameRole.White;
    } else {
        gameRole = GameRole.Black;
    }
    
    setDocumentTitle(gameRole, game);
    setUiForGame(gameRole, game);
    $('#resultInfoButton').on('click', () => onResultInfoButtonPressed(gameRole, game));
    $('#closeButton').on('click', closeResultPanel);
};

function setDocumentTitle(gameRole, game) {
    switch (gameRole) {
        case GameRole.White:
            document.title = `Review vs. ${game.blackPlayerName} - CoffeeChess`
            break;
        case GameRole.Black:
            document.title = `Review vs. ${game.whitePlayerName} - CoffeeChess`
            break;
        case GameRole.Spectator:
            document.title = `${game.whitePlayerName} vs. ${game.blackPlayerName} - CoffeeChess`;
            break;
        default:
            console.error(`Unsupported type of game role: ${gameRole}.`);
            break;
    }
}

function setUiForGame(gameRole, game) {
    chess = new Chess();
    board = new ChessBoard('reviewBoard', getConfig());
    historyManager = new HistoryManager(
        'reviewBoard',
            fen => board.position(fen), 
        viewHistoryTimers,
        getInitialTimeString(game)
    );

    setNamesAndRatings(game);
    setResult(game);
    setResultInfo(gameRole, game);
    setTimers(getInitialTimeString(game));
    
    for (let moveInfo of game.movesHistory) {
        const move = chess.move(moveInfo.san);
        if (move === null) {
            console.error("Something went wrong while processing SAN moves history.");
            break;
        }
        const timeAfterMove = getTimeAfterMoveString(moveInfo.timeAfterMove);
        const currentFen = chess.fen();
        historyManager.update(move, currentFen, timeAfterMove);
        historyManager.moveToLastMove();
    }
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

function onResultInfoButtonPressed(gameRole, game) {
    $('#modalOverlay').addClass('show');
    $('#resultPanel').css('display', 'flex');
    const [oldRating, newRating] = gameRole === GameRole.White
        ? [game.whitePlayerRating, game.whitePlayerNewRating]
        : [game.blackPlayerRating, game.blackPlayerNewRating];
    playRatingsChangeAnimation(oldRating, newRating);
}

function setResultInfo(gameRole, game) {
    const [panelColorClass, fontButtonsColorClass] = getClassesForResultPanel(gameRole, game);
    const resultTitle = getResultTitle(gameRole, game);
    
    $('#resultPanel').addClass(panelColorClass);
    $('#resultTitle')
        .text(resultTitle)
        .addClass(fontButtonsColorClass);
    $('#resultInfo')
        .text(getGameResultReasonText(gameRole, game))
        .addClass(fontButtonsColorClass);
    $('.result-info-title').addClass(fontButtonsColorClass);
    $('.result-info').addClass(fontButtonsColorClass);
    $('.result-button').addClass(fontButtonsColorClass);
    $('#timeControl').text(`${game.minutes}+${game.increment}`);
    $('#playedAt').text(`${getPlayedDate(game)}`);
}

function setTimers(time) {
    $('#whiteTimeLeft, #blackTimeLeft').text(time);
}


function getClassesForResultPanel(gameRole, game) {
    return gameRole === GameRole.Spectator
    || (gameRole === GameRole.White && game.gameResult === GameResult.WhiteWon)
    || (gameRole === GameRole.Black && game.gameResult === GameResult.BlackWon)
        ? ['milk', 'dark']
        : ['dark', 'milk'];
}

function getResultTitle(gameRole, game) {
    if (game.gameResult === GameResult.Draw)
        return 'Draw';
    
    switch (gameRole) {
        case GameRole.Black:
            return game.gameResult === GameResult.BlackWon ? 'You win' : 'You lose';
        case GameRole.White:
            return game.gameResult === GameResult.WhiteWon ? 'You win' : 'You lose';
        case GameRole.Spectator:
            return game.gameResult === GameResult.WhiteWon 
                ? `${game.whitePlayerName} wins` 
                : `${game.blackPlayerName} wins`;
    }
}

function getGameResultReasonText(gameRole, game) {
    if (game.gameResult === GameResult.Draw) {
        switch (game.gameResultReason) {
            case GameResultReason.FiftyMovesRule:
                return "by 50-moves rule.";
            case GameResultReason.Agreement:
                return "by agreement.";
            case GameResultReason.Stalemate:
                return "stalemate.";
            case GameResultReason.Threefold:
                return "by threefold repetition."
        }
    }
    
    if (game.gameResultReason === GameResultReason.Checkmate)
        return "checkmate.";

    const loserName = game.gameResult === GameResult.WhiteWon ? game.blackPlayerName : game.whitePlayerName;
    if (gameRole === GameRole.Spectator 
        || (gameRole === GameRole.White && game.gameResult === GameResult.WhiteWon)
        || (gameRole === GameRole.Black && game.gameResult === GameResult.BlackWon)) {
        return game.gameResultReason === GameResultReason.OpponentTimeRanOut 
            ? `${loserName}'s time is up.`
            : `${loserName} resigned.`;
    }
    
    return game.gameResultReason === GameResultReason.OpponentTimeRanOut
        ? "your time is up."
        : "you resigned.";
}

function getPlayedDate(game) {
    return new Date(game.playedDate)
        .toLocaleString("ru-RU", {
            day:    "2-digit",
            month:  "2-digit",
            year:   "numeric",
            hour:   "2-digit",
            minute: "2-digit",
            hour12: false,
            timeZone: "UTC"
        });
}

function getInitialTimeString(game) {
    let minutes = game.minutes;
    if (minutes < 10)
        minutes = `0${minutes}`;
    return `${minutes}:00`;
}

function getTimeAfterMoveString(time) {
    const timeParts = time.split(":");
    return `${timeParts[1]}:${timeParts[2].split('.')[0]}`;
}

function viewHistoryTimers(time, isWhite, isWhiteMoved) {
    if (isWhite) {
        $('#whiteTimeLeft').text(time ? time : "--:--");
    } else {
        $('#blackTimeLeft').text(time ? time : "--:--");
    }
    setTimerHighlighting(!isWhiteMoved);
}

const destroy = () => {
    chess = null;
    board = null;
    historyManager = null;
}

export default { init, destroy };