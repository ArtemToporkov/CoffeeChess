import { HistoryManager } from "./HistoryManager.js";
import { TimersManager } from "./TimersManager.js";

export class GameManager {
    board;
    isWhite;
    #shouldReturnToLive;
    #historyManager;
    #timersManager;
    #game;
    #gameId;
    #connection;
    #isWhiteTurn;
    
    constructor(connection, gameId, isWhite, totalMillisecondsLeft) {
        this.isWhite = isWhite; 
        this.board = ChessBoard('myBoard', this.#getConfig());
        
        this.#gameId = gameId;
        this.#connection = connection;
        
        this.#game = new Chess();
        this.#historyManager = new HistoryManager(this.#game, this.board);
        this.#timersManager = new TimersManager(totalMillisecondsLeft);
        this.#timersManager.start();
        
        this.#shouldReturnToLive = false;
        this.#isWhiteTurn = true;
    }
    
    updateGameState(pgn, newWhiteMillisecondsLeft, newBlackMillisecondsLeft) {
        this.#game.load_pgn(pgn);

        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);
        this.board.position(this.#game.fen());

        this.#isWhiteTurn = this.#game.turn() === 'w';

        this.#timersManager.updateTimers(newWhiteMillisecondsLeft, newBlackMillisecondsLeft, this.#isWhiteTurn);
        if (this.#timersManager.whiteMillisecondsLeft < 0 
            || this.#timersManager.whiteMillisecondsLeft < 0) {
            // TODO: implement losing game after time runs out
        }

        this.#historyManager.resetHistoryEvents();
    }
    
    #isMyTurn() {
        return (this.isWhite && this.#isWhiteTurn) 
            || (!this.isWhite && !this.#isWhiteTurn)
    }
    
    #getConfig() {
        const onDragStart = (source, piece, position, orientation) => {
            if ((!this.#isMyTurn() || this.#game.game_over()) 
                && this.#historyManager.currentPly === this.#game.history().length)
                return false;
        }

        const onDrop = (source, target) => {
            if (this.#historyManager.currentPly !== this.#game.history().length) {
                this.#shouldReturnToLive = true;
                return;
            }

            const move = this.#game.move({
                from: source,
                to: target,
                promotion: 'q'
            });

            if (move === null)
                return 'snapback';

            this.#connection.invoke("MakeMove", this.#gameId, move.from, move.to, move.promotion);
        }

        const onSnapEnd = () => {
            if (this.#shouldReturnToLive) {
                $('.history-selected').removeClass('history-selected');
                this.#shouldReturnToLive = false;
                this.#historyManager.setLastMoveToSelected();
            }
            this.board.position(this.#game.fen());
        }

        return {
            draggable: true,
            position: 'start',
            onDragStart: onDragStart,
            onDrop: onDrop,
            onSnapEnd: onSnapEnd,
            pieceTheme: '/img/chesspieces/{piece}.png',
            snapbackSpeed: 250
        };
    }
}