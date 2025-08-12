import { MusicPlayer } from "./MusicPlayer.js";

$(document).ready(() => {
    const playlist = Object.freeze([
            {
                title: "U Can Get It",
                author: "Proleter",
                audioSrc: "../tracks/Proleter-U-Can-Get-It/Proleter-U-Can-Get-It.mp3",
                coverSrc: "../tracks/Proleter-U-Can-Get-It/Proleter-U-Can-Get-It.jpg"
            },
            {
                title: "April Showers",
                author: "Proleter",
                audioSrc: "../tracks/Proleter-April-Showers/Proleter-April-Showers.mp3",
                coverSrc: "../tracks/Proleter-April-Showers/Proleter-April-Showers.jpg"
            }
        ]
    )
    const musicPlayer = new MusicPlayer(playlist);
});