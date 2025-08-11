$(document).ready(() => {
    let currentSongIdx = 0;
    const $musicPlayer = $('#musicPlayer');
    const $author = $('#songAuthor');
    const $title = $('#songTitle');
    const $cover = $('#songCover');
    loadTrack(currentSongIdx);
    
    function loadTrack(songIdx) {
        const track = playlist[songIdx];
        $author.text(track.author);
        $title.text(track.title);
        $musicPlayer.attr("src", track.audioSrc);
        $musicPlayer[0].play();
        $cover.attr("src", track.coverSrc);
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