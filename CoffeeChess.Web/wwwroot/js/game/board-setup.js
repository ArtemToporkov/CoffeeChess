import { HistoryManager } from "./history.js";

$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    
    let board = null;
    const game = new Chess();
    let historyManager = null;
    let shouldReturnToLive = false;

    function onDragStart(source, piece, position, orientation) {
        if ((!isMyTurn || game.game_over()) && historyManager.currentPly === game.history().length) 
            return false;
    }

    function onDrop(source, target) {
        if (historyManager.currentPly !== game.history().length) {
            shouldReturnToLive = true;
            return;
        }
        
        const move = game.move({
            from: source,
            to: target,
            promotion: 'q'
        });

        if (move === null) 
            return 'snapback';
        
        connection.invoke("MakeMove", gameId, move.from, move.to, move.promotion);
    }
    
    function onSnapEnd() {
        if (shouldReturnToLive) {
            isMyTurn = (isWhite && isWhiteTurn) || (!isWhite && !isWhiteTurn);
            $('.history-selected').removeClass('history-selected');
            shouldReturnToLive = false;
            historyManager.setLastMoveToSelected();
        }
        board.position(game.fen());
    }
    
    const config = {
        draggable: true,
        position: 'start',
        onDragStart: onDragStart,
        onDrop: onDrop,
        onSnapEnd: onSnapEnd,
        pieceTheme: '/img/chesspieces/{piece}.png',
        snapbackSpeed: 250
    };
    board = Chessboard('myBoard', config);
    historyManager = new HistoryManager(game, board);
    
    const whitePlayerInfo = JSON.parse(localStorage.getItem('whitePlayerInfo'));
    const blackPlayerInfo = JSON.parse(localStorage.getItem('blackPlayerInfo'));
    
    $('#whiteUsername').text(whitePlayerInfo.name);
    $('#blackUsername').text(blackPlayerInfo.name);
    $('#whiteRating').text(whitePlayerInfo.rating);
    $('#blackRating').text(blackPlayerInfo.rating);
    
    const isWhite = localStorage.getItem('isWhite') === "true";
    let isWhiteTurn = true;
    let isMyTurn = isWhite;
    if (!isWhite) {
        board.flip();
        $('.game-middle-panel').addClass('flipped');
    }
    
    let whiteMillisecondsLeft = localStorage.getItem('totalMillisecondsLeft');
    let blackMillisecondsLeft = localStorage.getItem('totalMillisecondsLeft');
    
    const timer = setInterval(() => {
        if (isWhiteTurn) {
            whiteMillisecondsLeft -= 1000;
        } else {
            blackMillisecondsLeft -= 1000;
        }
        if (whiteMillisecondsLeft < 0 || blackMillisecondsLeft < 0) {
            // TODO: implement losing a game after time runs out
        }
        updateTimers();
    }, 1000);
    
    function updateTimers() {
        const whiteTotalSecondsLeft = Math.floor(whiteMillisecondsLeft / 1000);
        const blackTotalSecondsLeft = Math.floor(blackMillisecondsLeft / 1000);

        const whiteMinutesLeft = Math.floor(whiteTotalSecondsLeft / 60);
        const whiteSecondsLeft = Math.floor(whiteTotalSecondsLeft % 60);
        const blackMinutesLeft = Math.floor(blackTotalSecondsLeft / 60);
        const blackSecondsLeft = Math.floor(blackTotalSecondsLeft % 60);
        
        const whiteMinutesText = whiteMinutesLeft < 10 ? `0${whiteMinutesLeft}` : `${whiteMinutesLeft}`;
        const whiteSecondsText = whiteSecondsLeft < 10 ? `0${whiteSecondsLeft}` : `${whiteSecondsLeft}`;
        
        const blackMinutesText = blackMinutesLeft < 10 ? `0${blackMinutesLeft}` : `${blackMinutesLeft}`;
        const blackSecondsText = blackSecondsLeft < 10 ? `0${blackSecondsLeft}` : `${blackSecondsLeft}`;
        
        $("#whiteTimeLeft").text(`${whiteMinutesText}:${whiteSecondsText}`);
        $("#blackTimeLeft").text(`${blackMinutesText}:${blackSecondsText}`);
    }
    
    connection.on("MakeMove", (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        game.load_pgn(pgn);

        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);
        board.position(game.fen());
        
        isWhiteTurn = game.turn() === 'w';
        isMyTurn = (isWhite && isWhiteTurn) || (!isWhite && !isWhiteTurn);
        whiteMillisecondsLeft = newWhiteMillisecondsLeft;
        blackMillisecondsLeft = newBlackMillisecondsLeft;
        
        updateTimers();
        historyManager.resetHistoryEvents();
    });
    
    connection.on("MoveFailed", (errorMessage) => {
        // TODO: undo move, display error message
    });
    
    connection.on("CriticalError", (errorMessage) => {
        // TODO: raise 500 with errorMessage
    });
    
    connection.start();
});