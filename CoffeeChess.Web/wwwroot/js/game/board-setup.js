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
        if (game.game_over()) 
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
        
        connection.invoke("MakeMove", gameId, game.fen())
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

    connection.on("MakeMove", (newFen) => {
        game.load(newFen);
        board.position(newFen);
    });
    
    async function startConnection() {
        await connection.start();
        connection.invoke("JoinChatGroup", gameId);
    }
    
    startConnection();
});