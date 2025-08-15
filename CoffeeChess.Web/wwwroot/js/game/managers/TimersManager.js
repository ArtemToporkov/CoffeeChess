export class TimersManager {
    #timer;
    #isWhiteTurn;
    whiteMillisecondsLeft;
    blackMillisecondsLeft;
    
    constructor(totalMillisecondsLeft) {
        this.#isWhiteTurn = true;
        this.#timer = null;
        this.whiteMillisecondsLeft = totalMillisecondsLeft;
        this.blackMillisecondsLeft = totalMillisecondsLeft;
        this.updateTimers();
    }
    
    destroy() {
        this.stop();
    }
    
    start() {
        this.#timer = setInterval(() => {
            if (this.#isWhiteTurn) {
                this.whiteMillisecondsLeft = Math.max(
                    this.whiteMillisecondsLeft - 1000, 0
                );
            } else {
                this.blackMillisecondsLeft = Math.max(
                    this.blackMillisecondsLeft - 1000, 0
                );
            }
            this.updateTimers();
            if (this.whiteMillisecondsLeft === 0 || this.blackMillisecondsLeft === 0) {
                this.stop();
            }
        }, 1000);
    }
    
    stop() {
        clearInterval(this.#timer);
        this.#timer = null;
    }

    updateTimers(whiteMillisecondsLeft = null, blackMillisecondsLeft = null, isWhiteTurn = null) {
        this.#isWhiteTurn = isWhiteTurn ?? this.#isWhiteTurn;
        this.whiteMillisecondsLeft = whiteMillisecondsLeft ?? this.whiteMillisecondsLeft;
        this.blackMillisecondsLeft = blackMillisecondsLeft ?? this.blackMillisecondsLeft;
        
        const whiteTotalSecondsLeft = Math.floor(this.whiteMillisecondsLeft / 1000);
        const blackTotalSecondsLeft = Math.floor(this.blackMillisecondsLeft / 1000);

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
}