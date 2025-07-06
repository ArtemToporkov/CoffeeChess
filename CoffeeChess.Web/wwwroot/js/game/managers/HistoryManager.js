import { highlightSquares, unhighlightSquares } from "../ui.js";

export class HistoryManager {
    #game;
    #historyViewGame;
    #board;
    currentPly = 0;
    
    constructor(game, board) {
        this.#game = game;
        this.#board = board;
        this.#historyViewGame = new Chess();

        $(document).on('keydown', e => {
            switch (e.key) {
                case 'ArrowLeft':
                    if (this.currentPly > 0) {
                        const ply = this.currentPly;
                        this.#moveToPly(ply - 1);
                        if (ply === 1) {
                            unhighlightSquares();
                        } else {
                            this.highlightMoveByPly(ply - 1);
                        }
                    }
                    break;
                case 'ArrowRight':
                    if (this.currentPly < this.#game.history().length) {
                        const ply = this.currentPly;
                        this.#moveToPly(ply + 1);
                        this.highlightMoveByPly(ply + 1);
                    }
                    break;
            }
        });
    }

    resetHistoryEvents() {
        this.currentPly = this.#game.history().length;
        const history = $('#history');
        history.children().slice(1).remove();
        const moves = this.#game.history();
        for (let i = 0; i < moves.length; i += 2) {
            const whiteMove = moves[i];
            const blackMove = i + 1 < moves.length ? moves[i + 1] : '';
            let historyRow = this.#getHistoryRow(i / 2 + 1, whiteMove, blackMove);

            historyRow.children().eq(1).on('click', () => {
                this.#moveToPly(i + 1);
                this.highlightMoveByPly(i + 1)
            });
            if (blackMove !== '') {
                historyRow.children().eq(2).on('click', () => {
                    this.#moveToPly(i + 2);
                    this.highlightMoveByPly(i + 2);
                });
            }

            history.append(historyRow);
        }
        this.moveToLastMove();
    }

    moveToLastMove() {
        const history = $('#history');
        const moves = this.#game.history();
        this.currentPly = moves.length;
        if (moves.length > 0) {
            const lastRow = history.children().last();
            const lastMoveElement = (moves.length % 2 === 0) ? lastRow.children().eq(2) : lastRow.children().eq(1);
            lastMoveElement.addClass('history-selected');
            lastMoveElement.addClass('show').one('animationend', () => {
                $(this).removeClass('show');
            })
        }
    }
    
    highlightMoveByPly(ply) {
        const move = this.getMoveByPly(ply);
        if (move === null)
            return;
        
        highlightSquares(move.from, move.to);
    }
    
    getMoveByPly(ply) {
        const history = this.#historyViewGame.history({ verbose: true });
        return this.#getMoveByPlyAndHistory(history, ply);
    }
   
    #getMoveByPlyAndHistory(history, ply) {
        if (ply > history.length || ply < 0) {
            return null;
        }
        
        return history[ply - 1];
    }

    #moveToPly(ply) {
        $('#myBoard .piece-417db, body > img.piece-417db').stop(true, true);

        $('.history-selected').removeClass('history-selected');
        const childNumber = ply % 2 === 0 ? 2 : 1;
        const moveNumber = Math.ceil(ply / 2);
        $('#history').children().eq(moveNumber).children().eq(childNumber).addClass('history-selected');
        this.#historyViewGame.reset();
        for (let i = 0; i < ply; i++) {
            this.#historyViewGame.move(this.#game.history()[i]);
        }
        this.currentPly = ply;
        this.#board.position(this.#historyViewGame.fen());
    }

    #getHistoryRow(number, whiteMove, blackMove) {
        return $('<div>', {
            class: 'history-row'
        }).append(
            $('<div>', {
                class: 'history-move-number',
                text: `${number}.`,
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
}