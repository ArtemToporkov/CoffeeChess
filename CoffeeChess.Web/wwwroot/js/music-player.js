$(document).ready(async () => {
    let currentSongIdx = 0;
    const $musicPlayer = $('#musicPlayer');
    const $author = $('#songAuthor');
    const $title = $('#songTitle');
    const $cover = $('#songCover');
    
    loadTrack(currentSongIdx);
    setPauseEvent();
    setNextPreviousEvent(true);
    setNextPreviousEvent(false);
    
    let canvas;
    createCanvas();
    let canvasContext = canvas.getContext('2d');
    
    let audioContext;
    let analyser;
    let source;
    let dataArray;
    let shuffledIndexes;
    
    updateAudioContext();
    drawVisualizer();
    
    function loadTrack(songIdx) {
        const track = playlist[songIdx];
        $author.text(track.author);
        $title.text(track.title);
        $musicPlayer.attr("src", track.audioSrc);
        $musicPlayer[0].play();
        $cover.attr("src", track.coverSrc);
    }
    
    function setPauseEvent() {
        const $button = $('#pauseButton');
        const $img = $button.find('img').first();
        $button.on('pointerdown', e => {
            if ($img.hasClass('pause')) {
                $img.removeClass('pause').addClass('play');
                $img.attr('src', '/img/song-control-triangle.png');
            } else if ($img.hasClass('play')) {
                $img.removeClass('play').addClass('pause');
                $img.attr('src', '/img/song-control-pause.png');
            } else {
                throw Error("Can't define eather song is paused or not.");
            }
            $button.addClass('pressed').on('transitionend', 
                () => $button.removeClass('pressed')
            );
        });
    }

    function setNextPreviousEvent(isPrevious) {
        const $button = isPrevious ? $('#previousButton') : $('#nextButton');
        const children = $button.children('img').get().map(child => $(child));
        const childrenCount = children.length;
        const delay = 100;
        $button.on('pointerdown', e => {
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

    function updateAudioContext() {
        audioContext = new (window.AudioContext || window.webkitAudioContext)();
        analyser = audioContext.createAnalyser();
        analyser.fftSize = 64;
        source = audioContext.createMediaElementSource($musicPlayer[0]);

        source.connect(analyser);
        analyser.connect(audioContext.destination);
        
        const bufferLength = analyser.frequencyBinCount;
        dataArray = new Uint8Array(bufferLength);
    }
    
    function drawVisualizer() {
        requestAnimationFrame(drawVisualizer);
        analyser.getByteFrequencyData(dataArray);
        canvasContext.clearRect(0, 0, canvas.width, canvas.height);

        const linesCount = Math.min(analyser.frequencyBinCount, 32)
        const barWidth = (canvas.width / (linesCount));
        const rightOffset = 12;
        let barHeight;
        let drawCoordinate = 0;
        
        if (!shuffledIndexes)
            createShuffledIndexes(linesCount - rightOffset)
        canvasContext.fillStyle = '#2b1100';
        
        shuffledIndexes.forEach(i => {
            barHeight = dataArray[i] / 5;
            canvasContext.fillRect(drawCoordinate, canvas.height - barHeight / 2, barWidth, barHeight / 2);
            drawCoordinate += barWidth + 2;
        });
    }
    
    function createCanvas() {
        const $container = $('.song-title-and-visualizer-container');
        canvas = document.createElement('canvas');
        canvas.width = $container.width();
        canvas.height = $container.height() / 3;
        canvas.id = 'visualizer';
        canvas.classList.add('visualizer');
        $container.append(canvas);
    }
    
    function createShuffledIndexes(length) {
        shuffledIndexes = Array.from({ length: length }, (_, i) => i);
        for (let i = shuffledIndexes.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [shuffledIndexes[i], shuffledIndexes[j]] = [shuffledIndexes[j], shuffledIndexes[i]];
        }
    }
});

const playlist = Object.freeze([
        {
            title: "U Can Get It",
            author: "Proleter",
            audioSrc: "../tracks/Proleter-U-Can-Get-It/Proleter-U-Can-Get-It.mp3",
            coverSrc: "../tracks/Proleter-U-Can-Get-It/Proleter-U-Can-Get-It.jpg"
        }
    ]
)