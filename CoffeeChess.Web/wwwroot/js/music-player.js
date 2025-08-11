$(document).ready(async () => {
    let currentSongIdx = 0;
    const $musicPlayer = $('#musicPlayer');
    const $author = $('#songAuthor');
    const $title = $('#songTitle');
    const $cover = $('#songCover');
    
    loadTrack(currentSongIdx);
    
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