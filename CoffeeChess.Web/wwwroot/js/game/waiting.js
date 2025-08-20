import { GameHubEvents } from "./enums/GameHubEvents.js";
import { animateSearching } from "./ui.js";
import { ajaxNavigator } from "../site.js"

let connection;

const init = async () => {
    await document.fonts.ready;
    animateSearching();
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    connection.on(GameHubEvents.GameStarted, async (gameId, isWhite, whitePlayerInfo,
                                              blackPlayerInfo, totalMillisecondsLeft) => {
        localStorage.setItem("totalMillisecondsLeft", totalMillisecondsLeft);
        localStorage.setItem("isWhite", isWhite);
        localStorage.setItem("whitePlayerInfo", JSON.stringify(whitePlayerInfo));
        localStorage.setItem("blackPlayerInfo", JSON.stringify(blackPlayerInfo));
        await ajaxNavigator.loadContent(`/Game/Play/${gameId}`);
    });
    
    await connection.start();
    const challengeSettingsParams = new URLSearchParams(window.location.search);
    const keys = ['minutes', 'increment', 'colorPreference', 'minRating', 'maxRating'];

    const challengeSettings = keys.reduce((obj, key) => {
        const value = parseInt(challengeSettingsParams.get(key));
        if (!isNaN(value)) 
            obj[key] = value;
        return obj;
    }, {});

    await $.ajax({
        url: `/Game/GameCreation/QueueOrFindChallenge`,
        method: 'POST',
        data: JSON.stringify(challengeSettings),
        contentType: 'application/json',
    });
};

const destroy = async () => {
    await connection.stop();
    connection = null;
};

export default { init, destroy };

