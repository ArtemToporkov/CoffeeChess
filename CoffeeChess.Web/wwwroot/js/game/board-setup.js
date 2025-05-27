$(document).ready(function () {
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
    }
    
    function onSnapEnd() {
        board.position(game.fen());
    }
    
    var config = {
        draggable: true,
        position: 'start',
        onDragStart: onDragStart,
        onDrop: onDrop,
        onSnapEnd: onSnapEnd,
        pieceTheme: '../img/chesspieces/{piece}.png'
    };
    board = Chessboard('myBoard', config);
});