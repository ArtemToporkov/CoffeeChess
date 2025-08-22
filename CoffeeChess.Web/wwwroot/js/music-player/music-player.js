import { MusicPlayer } from "./MusicPlayer.js";
import { songCache } from "./song-cache.js";

$(document).ready(async () => {
    let playlist = await songCache.getPlaylist();
    if (!playlist || playlist.length === 0) {
        playlist = await $.ajax({
            url: "/Songs/All",
            type: "GET",
            dataType: "json"
        });
        await songCache.cachePlaylist(playlist);
    }
    const musicPlayer = new MusicPlayer(playlist);
    await musicPlayer.start();
});