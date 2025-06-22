$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    
    let board = null;
    const game = new Chess();
    const historyViewGame = new Chess();
    let currentPly = 0;
    let currentMoveIndex = -1;
    let shouldReturnToLive = false;

    function onDragStart(source, piece, position, orientation) {
        if (!isMyTurn || game.game_over()) 
            return false;
    }

    function onDrop(source, target) {
        if (currentPly !== game.history().length) {
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
            currentPly = game.history().length;
            isMyTurn = (isWhite && isWhiteTurn) || (!isWhite && !isWhiteTurn);
            $('.history-selected').removeClass('history-selected');
            setLastMoveToSelected();
            shouldReturnToLive = false;
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
    
    function updateHistory() {
        const history = $('#history');
        history.children().slice(1).remove();
        const moves = game.history();
        for (let i = 0; i < moves.length; i += 2) {
            const whiteMove = moves[i];
            const blackMove = i + 1 < moves.length ? moves[i + 1] : '';
            let historyRow = getHistoryRow(i / 2 + 1, whiteMove, blackMove);
            
            historyRow.children().eq(1).on('click', () => {
                undoToPly(i + 1);
            });
            if (blackMove !== '') {
                historyRow.children().eq(2).on('click', () => {
                   undoToPly(i + 2); 
                });
            }
            
            history.append(historyRow);
        }
        setLastMoveToSelected();
    }
    
    function undoToPly(ply) {
        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);
        
        $('.history-selected').removeClass('history-selected');
        const childNumber = ply % 2 === 0 ? 2 : 1; 
        const moveNumber = Math.ceil(ply / 2);
        $('#history').children().eq(moveNumber).children().eq(childNumber).addClass('history-selected');
        historyViewGame.reset();
        for (let i = 0; i < ply; i++) {
            historyViewGame.move(game.history()[i]);
        }
        currentPly = ply;
        isMyTurn = true;
        board.position(historyViewGame.fen());
    }
    
    function setLastMoveToSelected() {
        const history = $('#history');
        const moves = game.history();
        if (moves.length > 0) {
            const lastRow = history.children().last();
            const lastMoveElement = (moves.length % 2 === 0) ? lastRow.children().eq(2) : lastRow.children().eq(1);
            lastMoveElement.addClass('history-selected');
        }
    }
    
    function getHistoryRow(number, whiteMove, blackMove) {
        return $('<div>', {
            class: 'history-row'
        }).append(
            $('<div>', {
                class: 'history-move-number',
                text: number,
            }).css('cursor', 'default'),
            $('<div>', {
                class: 'history-move',
                text: whiteMove
            }).css('cursor', whiteMove === 'White' ? 'default' : 'pointer'),
            $('<div>', {
                class: 'history-move',
                text: blackMove
            }).css('cursor', ['', 'Black'].includes(blackMove) ? 'default' : 'pointer')
        );
    }
    
    connection.on("MakeMove", (pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        game.load_pgn(pgn);

        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);
        board.position(game.fen());
        
        currentPly += 1;
        isWhiteTurn = game.turn() === 'w';
        isMyTurn = (isWhite && isWhiteTurn) || (!isWhite && !isWhiteTurn);
        whiteMillisecondsLeft = newWhiteMillisecondsLeft;
        blackMillisecondsLeft = newBlackMillisecondsLeft;
        
        updateTimers();
        updateHistory();
    });
    
    connection.on("MoveFailed", (errorMessage) => {
        // TODO: undo move, display error message
    });
    
    connection.on("CriticalError", (errorMessage) => {
        // TODO: raise 500 with errorMessage
    });
    
    $(document).on('keydown', e => {
       switch (e.key) {
           case 'ArrowLeft':
               if (currentPly > 1) {
                   undoToPly(currentPly - 1);
               }
               break;
           case 'ArrowRight':
               if (currentPly < game.history().length) {
                   undoToPly(currentPly + 1);
               }
               break;
       } 
    });
    
    async function startConnection() {
        await connection.start();
    }
    
    startConnection();
});