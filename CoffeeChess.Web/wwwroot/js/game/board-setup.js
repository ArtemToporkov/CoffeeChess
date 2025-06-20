$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    
    let board = null;
    const game = new Chess();

    function onDragStart(source, piece, position, orientation) {
        if (!isMyTurn || game.game_over()) 
            return false;
    }

    function onDrop(source, target) {
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

        $("#whiteTimeLeft").text(`${whiteMinutesLeft}:${whiteSecondsLeft}`);
        $("#blackTimeLeft").text(`${blackMinutesLeft}:${blackSecondsLeft}`);
    }
    
    connection.on("MakeMove", (newFen, newIsWhiteTurn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) => {
        isWhiteTurn = newIsWhiteTurn;
        isMyTurn = (isWhite && isWhiteTurn) || (!isWhite && !isWhiteTurn);
        whiteMillisecondsLeft = newWhiteMillisecondsLeft;
        blackMillisecondsLeft = newBlackMillisecondsLeft;
        updateTimers();
        game.load(newFen);
        board.position(newFen);
    });
    
    connection.on("MoveFailed", (errorMessage) => {
        // TODO: undo move, display error message
    });
    
    connection.on("CriticalError", (errorMessage) => {
        // TODO: raise 500 with errorMessage
    });
    
    async function startConnection() {
        await connection.start();
    }
    
    startConnection();
});