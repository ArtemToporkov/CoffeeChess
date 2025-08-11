import { highlightSquares, unhighlightSquares, setTimerHighlighting } from "../ui.js";

export class HistoryManager {
    currentPly;
    #boardName;
    #movesHistory;
    #viewHistoryByBoard;
    #viewHistoryTimerCallback;
    
    constructor(
        boardName, 
        viewHistoryBoardCallback, 
        viewHistoryTimerCallback = null,
        fromTime = null,
        fromFen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq — 0 1"
    ) {
        this.#boardName = boardName;
        this.#bindArrowKeys();
        this.#viewHistoryByBoard = viewHistoryBoardCallback;
        this.currentPly = 0;
        this.#movesHistory = [{move: null, fen: fromFen, timeAfterMove: fromTime}];
        this.#viewHistoryTimerCallback = viewHistoryTimerCallback;
    }
    
    moveToLastMove() {
        this.currentPly = this.#movesHistory.length - 1;
        const currentPly = this.currentPly;
        this.#moveToPlyAndHighlight(currentPly);
        if (this.#viewHistoryTimerCallback) {
            this.#setTimersAfterMove(currentPly);
        }
    }
    
    update(move, fen, timeAfterMove = null) {
        $('.history-selected').removeClass('history-selected');
        this.#movesHistory.push({move: move, fen: fen, timeAfterMove: timeAfterMove});
        this.currentPly = this.#movesHistory.length - 1;
        
        const plyToMoveTo = this.currentPly;
        const viewHistoryCallback = () => {
            this.#moveToPlyAndHighlight(plyToMoveTo);
            if (this.#viewHistoryTimerCallback !== null) {
                this.#setTimersAfterMove(plyToMoveTo);
            }
        };
        
        const $lastRow = this.#getLastRow();
        
        if ($lastRow !== null && this.#movesHistory.length % 2 !== 0) {
            $lastRow.children()
                .last()
                .text(move.san)
                .addClass('history-selected')
                .css('cursor', 'pointer')
                .on('click', viewHistoryCallback);
            return;
        }
        
        const $row = this.#getNewHistoryRow(Math.ceil(this.currentPly / 2), move);
        $row.children()
            .eq(1)
            .addClass('history-selected')
            .on('click', viewHistoryCallback);
        $('#history').append($row);
        $row.addClass('show').one('animationend', () => $row.removeClass('show'));
    }
    
    #getLastRow() {
        const $children = $('#history').children();
        if ($children.length <= 1)
            return null;
        return $children.last();
    }
    
    #moveToPlyAndHighlight(ply) {
        $(`#${this.#boardName} .piece-417db, body > img.piece-417db`).stop(true, true);
        $('.history-selected').removeClass('history-selected');
        this.#viewHistoryByBoard(this.#movesHistory[ply].fen);
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
            switch (e.key) {
                case 'ArrowLeft':
                    e.preventDefault();
                    if (this.currentPly > 0) {
                        const ply = this.currentPly;
                        this.#moveToPlyAndHighlight(ply - 1);
                        if (this.#viewHistoryTimerCallback !== null)
                            this.#setTimersAfterMove(ply - 1);
                    }
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    if (this.currentPly < this.#movesHistory.length - 1) {
                        const ply = this.currentPly;
                        this.#moveToPlyAndHighlight(ply + 1);
                        if (this.#viewHistoryTimerCallback !== null)
                            this.#setTimersAfterMove(ply + 1);
                    }
                    break;
            }
        });
    }
    
    #setTimersAfterMove(ply) {
        if (ply === 0)
        {
            const time = this.#movesHistory[ply].timeAfterMove;
            this.#viewHistoryTimerCallback(time, true);
            this.#viewHistoryTimerCallback(time, false);
            return;
        }
        const isWhiteMoved = ply % 2 !== 0;
        const [whiteTime, blackTime] = isWhiteMoved
            ? [this.#movesHistory[ply].timeAfterMove, this.#movesHistory[ply - 1].timeAfterMove]
            : [this.#movesHistory[ply - 1].timeAfterMove, this.#movesHistory[ply].timeAfterMove];
        this.#viewHistoryTimerCallback(whiteTime, true, isWhiteMoved);
        this.#viewHistoryTimerCallback(blackTime, false, isWhiteMoved);
    }
}