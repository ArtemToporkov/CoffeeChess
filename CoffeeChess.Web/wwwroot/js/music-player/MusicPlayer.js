import { Visualizer } from "./Visualizer.js";
import { songCache } from "./song-cache.js";

export class MusicPlayer {
    #currentSongIdxValue;

    get #currentSongIdx() {
        return this.#currentSongIdxValue;
    }
    set #currentSongIdx(idx) {
        let normalizedIdx = idx % this.#playlist.length;
        if (normalizedIdx < 0)
            normalizedIdx += this.#playlist.length;
        this.#currentSongIdxValue = normalizedIdx;
    }
    
    #musicPlayer;
    #playlist;
    #$authorWrapper;
    #$titleWrapper;
    #$cover;
    #isPaused;

    #audioContext;
    #audioAnalyzer;
    #source;
    #dataArray;
    
    #visualizer;
    
    constructor(playlist) {
        this.#playlist = playlist;
        this.#musicPlayer = $('#musicPlayer')[0];
        this.#musicPlayer.crossOrigin = "anonymous";
        this.#$authorWrapper = $('#songAuthorWrapper');
        this.#$titleWrapper = $('#songTitleWrapper');
        this.#$cover = $('#songCover');
        this.#fillSongsList();
        
        const canvas = this.#createCanvas();
        const canvasContext = canvas.getContext('2d');
        this.#setupAudioContext();
        this.#visualizer = new Visualizer(this.#audioAnalyzer, canvas, canvasContext);

        this.#setPauseEvents();
        this.#setNextPreviousEvents(true);
        this.#setNextPreviousEvents(false);
        this.#setExtendButton();
        this.#setHideButton();

        this.#musicPlayer.addEventListener('ended', async () => {
            await this.#loadSong(this.#currentSongIdx + 1);
        });
    }
    
    async start() {
        await this.#loadSong(0);
    }

    async #loadSong(songIdx) {
        if (this.#currentSongIdx === songIdx)
            return;
        this.#currentSongIdx = songIdx;
        this.#selectSong(this.#currentSongIdx);
        const song = this.#playlist[this.#currentSongIdx];

        const cachedFiles = await songCache.getSongFiles(song.songId);
        let songUrl;
        let coverUrl;

        if (cachedFiles && cachedFiles.audio && cachedFiles.cover) {
            songUrl = URL.createObjectURL(cachedFiles.audio);
            coverUrl = URL.createObjectURL(cachedFiles.cover);
        } else {
            const songResponse = await $.ajax({
                url: `/Songs/Audio/${song.songId}`,
                type: 'GET',
                xhrFields: { responseType: 'blob' }
            });
            const coverResponse = await $.ajax({
                url: `/Songs/Cover/${song.songId}`,
                type: 'GET',
                xhrFields: { responseType: 'blob' }
            });

            songUrl = URL.createObjectURL(songResponse);
            coverUrl = URL.createObjectURL(coverResponse);
            await songCache.cacheSongFiles(song.songId, songResponse, coverResponse);
        }
        
        this.#loadSongInfo(coverUrl, song.author, song.title);
        this.#musicPlayer.src = songUrl;
        this.#musicPlayer.play();
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
        children[childrenCount - 1].on('transitionend', () => {
            children.forEach($child => $child.removeClass('pressed'))
        });
        $button.on('pointerdown', async e => {
            for (let i = 0; i < childrenCount; i++) {
                const $child = children[i];
                setTimeout(() => {
                    $child.addClass('pressed')
                }, i * delay);
            }
            await this.#loadSong(
                forPrevious 
                    ? (this.#currentSongIdx - 1) % this.#playlist.length
                    : (this.#currentSongIdx + 1) % this.#playlist.length
            )
            if (this.#isPaused)
                this.#play();
        });
    }
    
    #loadSongInfo(coverSrc, author, title) {
        const delay = 100;
        const songInfoAndVisualizerContainerWidth = $('.song-title-and-visualizer-container').width();
        const animationDuration = $('.scrolling-text').css('--animation-duration');
        console.log(animationDuration);
        const toggleScrolling = $text => {
            const textWidth = $text.width();
            $text.toggleClass('scrolling-text', textWidth > songInfoAndVisualizerContainerWidth);
            $text.css('--animation-duration', textWidth > songInfoAndVisualizerContainerWidth 
                ? `${textWidth / 20}s` 
                : animationDuration);
        }
        const toChange = [
            {$el: this.#$cover, changeFunc: () => this.#$cover.attr('src', coverSrc)},
            {$el: this.#$authorWrapper, changeFunc: () => {
                const $text = this.#$authorWrapper.find('span');
                $text.text(author);
                toggleScrolling($text);
            }}, 
            {$el: this.#$titleWrapper, changeFunc: () => {
                const $text = this.#$titleWrapper.find('span');
                $text.text(title);
                toggleScrolling($text);
                $text.toggleClass('scrolling-text', $text.width() > songInfoAndVisualizerContainerWidth);
            }}
        ]
        toChange.forEach((change, i) => {
            setTimeout(() => {
                change.$el.addClass('hide')
                    .one('transitionend', () => {
                        change.changeFunc();
                        change.$el.removeClass('hide');
                    });
            }, i * delay);
        })
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
    
    #setExtendButton() {
        const $button = $('#extendButton');
        $button.on('pointerdown', e => {
            if ($button.hasClass('pressed')) {
                $('.music-player-panel').removeClass('extended');
                $button.removeClass('pressed');
            }
            else {
                $('.music-player-panel').addClass('extended');
                $button.addClass('pressed');
            }
        });
    }
    
    #setHideButton() {
        const $panel = $('.music-player-panel');
        const $button = $('#hideButton');
        $button.on('pointerdown', e => {
            $panel.toggleClass('hide');
        });
    }
    
    #fillSongsList() {
        const $songsList = $('#songsList');
        const delayForScrollingText = 300;
        this.#playlist.forEach((song, i) => {
            const $songInfo = $('<div>')
                .addClass('list-song-info')
                .append(
                    $('<div>')
                        .addClass('list-song-title-author-wrapper')
                        .append(
                            $('<span>')
                                .addClass('list-song-author')
                                .text(song.author)
                        )
                )
                .append(
                    $('<div>')
                        .addClass('list-song-title-author-wrapper')
                        .append(
                            $('<span>')
                                .addClass('list-song-title')
                                .text(song.title)
                        )
                )
            $songsList.append($songInfo);
            const $wrapper = $songInfo.find('.list-song-title-author-wrapper');
            const $author = $songInfo.find('.list-song-author');
            const authorWidth = $author.width();
            const $title = $songInfo.find('.list-song-title');
            const titleWidth = $title.width();
            const wrapperWidth = $wrapper.width();
            const pixelsPerSecondDivider = 25;
            if (authorWidth > wrapperWidth)
                setTimeout(() => $author
                        .addClass('scrolling-text')
                        .css('--animation-duration', `${authorWidth / pixelsPerSecondDivider}s`), 
                    delayForScrollingText * i
                );
            if (titleWidth > wrapperWidth)
                setTimeout(() => $title
                        .addClass('scrolling-text')
                        .css('--animation-duration', `${titleWidth / pixelsPerSecondDivider}s`), 
                    delayForScrollingText * i
                );
            $songInfo.on('pointerdown', async () => {
                await this.#loadSong(i);
            });
        });
    }
    
    #selectSong(idx) {
        const $songsList = $('#songsList');
        const $songInfo = $songsList.children().eq(idx);
        $('.song-selected').removeClass('song-selected');
        $songInfo.addClass('song-selected');
    }
}