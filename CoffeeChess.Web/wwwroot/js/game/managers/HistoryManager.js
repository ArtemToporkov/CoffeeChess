import { highlightSquares, unhighlightSquares } from "../ui.js";

export class HistoryManager {
    currentPly;
    board;
    #movesHistory;
    
    constructor(board, fromFen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq — 0 1") {
        this.#bindArrowKeys();
        this.board = board;
        this.currentPly = 0;
        this.#movesHistory = [{move: null, fen: fromFen}];
    }
    
    moveToLastMove() {
        this.currentPly = this.#movesHistory.length - 1;
        this.#moveToPlyAndHighlight(this.currentPly);
    }
    
    update(move, fen) {
        $('.history-selected').removeClass('history-selected');
        this.#movesHistory.push({move: move, fen: fen});
        this.currentPly = this.#movesHistory.length - 1;
        
        const plyToMoveTo = this.currentPly;
        const moveCallback = () => this.#moveToPlyAndHighlight(plyToMoveTo);
        
        const $lastRow = this.#getLastRow();
        
        if ($lastRow !== null && !this.#checkBlackMoveInRow($lastRow)) {
            $lastRow.children()
                .last()
                .text(move.san)
                .addClass('history-selected')
                .css('cursor', 'pointer')
                .on('click', moveCallback);
            return;
        }
        
        const $row = this.#getNewHistoryRow(Math.ceil(this.currentPly / 2), move);
        $row.children()
            .eq(1)
            .addClass('history-selected')
            .on('click', moveCallback);
        $('#history').append($row);
    }
    
    #getLastRow() {
        const $children = $('#history').children();
        if ($children.length <= 1)
            return null;
        return $children.last();
    }
    
    #checkBlackMoveInRow($row) {
        return $row.children().last().text() !== '';
    }
    
    #moveToPlyAndHighlight(ply) {
        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);
        $('.history-selected').removeClass('history-selected');
        this.board.position(this.#movesHistory[ply].fen);
        this.currentPly = ply;
        unhighlightSquares();
        if (ply > 0) {
            const $row = $('#history').children().eq(Math.ceil(this.currentPly / 2));
            const toSelect = ply % 2 === 0 ? 2 : 1;
            $row.children().eq(toSelect).addClass('history-selected');
            
            const move = this.#movesHistory[ply].move;
            highlightSquares(move.from, move.to);
        }
    }

    #getNewHistoryRow(number, move) {
        return $('<div>', {
            class: 'history-row'
        }).append(
            $('<div>', {
                class: 'history-move-number',
                text: `${number}.`,
            }).css('cursor', 'default'),
            $('<div>', {
                class: 'history-move',
                text: move.san
            }).css('cursor', 'pointer'),
            $('<div>', {
                class: 'history-move',
                text: ''
            })
        );
    }
    
    #bindArrowKeys() {
        $(document).on('keydown', e => {
            e.preventDefault();
            switch (e.key) {
                case 'ArrowLeft':
                    if (this.currentPly > 0) {
                        const ply = this.currentPly;
                        this.#moveToPlyAndHighlight(ply - 1);
                    }
                    break;
                case 'ArrowRight':
                    if (this.currentPly < this.#movesHistory.length - 1) {
                        const ply = this.currentPly;
                        this.#moveToPlyAndHighlight(ply + 1);
                    }
                    break;
            }
        });
    }
}