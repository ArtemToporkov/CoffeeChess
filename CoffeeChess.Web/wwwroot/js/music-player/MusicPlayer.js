import { Visualizer } from "./Visualizer.js";

export class MusicPlayer {
    #currentSongIdx;
    #musicPlayer;
    #playlist;
    #$author;
    #$title;
    #$cover;
    #isPaused;

    #audioContext;
    #audioAnalyzer;
    #source;
    #dataArray;
    
    #visualizer;
    
    constructor(playlist) {
        this.#currentSongIdx = 0;
        this.#musicPlayer = $('#musicPlayer')[0];
        this.#musicPlayer.crossOrigin = "anonymous";
        this.#playlist = playlist;
        this.#$author = $('#songAuthor');
        this.#$title = $('#songTitle');
        this.#$cover = $('#songCover');
        
        const canvas = this.#createCanvas();
        const canvasContext = canvas.getContext('2d');
        this.#setupAudioContext();
        this.#visualizer = new Visualizer(this.#audioAnalyzer, canvas, canvasContext);
        this.#loadSong(0);

        this.#setPauseEvents();
        this.#setNextPreviousEvents(true);
        this.#setNextPreviousEvents(false);
    }

    #setPauseEvents() {
        const $button = $('#pauseButton');
        $button.on('pointerdown', e => {
            if (!this.#isPaused) {
                this.#pause();
            } else
                this.#play();
            $button.addClass('pressed').on('transitionend',
                () => $button.removeClass('pressed')
            );
        });
    }
    
    #pause() {
        const $button = $('#pauseButton');
        const $img = $button.find('img').first();
        $img.removeClass('pause').addClass('play');
        $img.attr('src', '/img/song-control-triangle.png');
        this.#musicPlayer.pause();
        this.#isPaused = true;
    }
    
    #play() {
        const $button = $('#pauseButton');
        const $img = $button.find('img').first();
        $img.removeClass('play').addClass('pause');
        $img.attr('src', '/img/song-control-pause.png');
        this.#musicPlayer.play();
        this.#isPaused = false;
    }

    #setNextPreviousEvents(forPrevious) {
        const $button = forPrevious ? $('#previousButton') : $('#nextButton');
        const children = $button.children('img').get().map(child => $(child));
        const childrenCount = children.length;
        const delay = 100;
        $button.on('pointerdown', e => {
            this.#loadSong(
                forPrevious 
                    ? (this.#currentSongIdx - 1) % this.#playlist.length
                    : (this.#currentSongIdx + 1) % this.#playlist.length
            )
            if (this.#isPaused)
                this.#play();
            for (let i = 0; i < childrenCount; i++) {
                const $child = children[i];
                setTimeout(() => {
                    $child.addClass('pressed')
                }, i * delay);
            }
        });
        children[childrenCount - 1].on('transitionend', () => {
            children.forEach($child => $child.removeClass('pressed'))
        });
    }

    #loadSong(songIdx) {
        const track = this.#playlist[songIdx];
        this.#$author.text(track.author);
        this.#$title.text(track.title);
        this.#$cover.attr("src", track.coverSrc);
        this.#musicPlayer.src = track.audioSrc;
        this.#musicPlayer.play();
        this.#currentSongIdx = songIdx;
    }

    #setupAudioContext() {
        this.#audioContext = new (window.AudioContext || window.webkitAudioContext)();
        this.#audioAnalyzer = this.#audioContext.createAnalyser();
        this.#audioAnalyzer.fftSize = 64;
        this.#source = this.#audioContext.createMediaElementSource(this.#musicPlayer);

        this.#source.connect(this.#audioAnalyzer);
        this.#audioAnalyzer.connect(this.#audioContext.destination);

        const barsCount = this.#audioAnalyzer.frequencyBinCount;
        this.#dataArray = new Uint8Array(barsCount);
    }

    #createCanvas() {
        const $container = $('.song-title-and-visualizer-container');
        const canvas = document.createElement('canvas');
        canvas.width = $container.width();
        canvas.height = $container.height() / 3;
        canvas.id = 'visualizer';
        canvas.classList.add('visualizer');
        $container.append(canvas);
        return canvas;
    }
}