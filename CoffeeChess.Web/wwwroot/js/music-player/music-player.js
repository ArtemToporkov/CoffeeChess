import { MusicPlayer } from "./MusicPlayer.js";

$(document).ready(async () => {
    const playlist = await $.ajax({
       url: "/Songs/All",
       type: "GET",
       dataType: "json" 
    });
    const musicPlayer = new MusicPlayer(playlist);
    await musicPlayer.start();
});