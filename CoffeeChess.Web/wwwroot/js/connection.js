import { ajaxNavigator } from "./site.js";
import { GameHubEvents } from "./game/enums/GameHubEvents.js";

if (!window._connection) {
    window._connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    window._connection.on(GameHubEvents.GameStarted, async gameId => {
        await ajaxNavigator.loadContent(`/Game/Play/${gameId}`);
    });

    await window._connection.start();
}
export const connection = window._connection;
