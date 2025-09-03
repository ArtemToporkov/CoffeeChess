import { ajaxNavigator } from "./site.js";
import { GameHubEvents } from "./game/enums/GameHubEvents.js";

export const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on(GameHubEvents.GameStarted, async gameId => {
    await ajaxNavigator.loadContent(`/Game/Play/${gameId}`);
});

await connection.start();
