export class Visualizer {
    #audioDataArray;
    #audioAnalyzer;
    #canvas;
    #canvasContext;
    #shuffledIndexes;
    
    constructor(audioAnalyzer, canvas, canvasContext) {
        this.#audioAnalyzer = audioAnalyzer;
        this.#canvas = canvas;
        this.#canvasContext = canvasContext;
        const linesCount = Math.min(this.#audioAnalyzer.frequencyBinCount, 32);
        this.#audioDataArray = new Uint8Array(linesCount);
        const barWidth = (this.#canvas.width / linesCount);
        const rightOffset = 10;
        this.#shuffledIndexes = this.#createShuffledIndexes(linesCount - rightOffset);
        this.#loopVisualizer(barWidth);
    }
    
    #loopVisualizer(barWidth) {
        requestAnimationFrame(() => this.#loopVisualizer(barWidth));
        this.#drawVisualizer(barWidth);
    }

    #drawVisualizer(barWidth) {
        this.#audioAnalyzer.getByteFrequencyData(this.#audioDataArray);
        this.#canvasContext.clearRect(0, 0, this.#canvas.width, this.#canvas.height);
        
        let drawCoordinate = 0;
        this.#canvasContext.fillStyle = '#2b1100';

        this.#shuffledIndexes.forEach(i => {
            const barHeight = this.#audioDataArray[i] / 5 + 5;
            this.#canvasContext.fillRect(
                drawCoordinate, this.#canvas.height - barHeight / 2, barWidth, barHeight / 2);
            drawCoordinate += barWidth + 2;
        });
    }

    #createShuffledIndexes(length) {
        const shuffledIndexes = Array.from({ length: length }, (_, i) => i);
        for (let i = shuffledIndexes.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [shuffledIndexes[i], shuffledIndexes[j]] = [shuffledIndexes[j], shuffledIndexes[i]];
        }
        return shuffledIndexes;
    }
}