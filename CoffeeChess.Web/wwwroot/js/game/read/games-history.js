import { GameResult } from "../enums/GameResult.js";

$(document).ready(async () => {
    const pageSize = 10;
    
    const gamesCount = await $.ajax({
        url: '/GamesHistory/GetCount',
        method: 'GET',
        dataType: 'json'
    });
    
    const username = $('#username').text();
    if (!username) {
        console.error("You are not authenticated.");
        return;
    }
    buildPaginationPanel(username, pageSize, gamesCount);
    await getGamesAndAppendToHistory(username, 1, pageSize);
});

function buildPaginationPanel(username, pageSize, totalCount) {
    const $paginationPanel = $('#paginationPanel');
    $paginationPanel.empty();
    const pagesCount = Math.ceil(totalCount / pageSize);
    for (let i = 0; i < pagesCount; i++) {
        const $button = buildPaginationButton(i + 1, i === 0);
        $paginationPanel.append($button);
        $button.on('click', async e => {
            $('.pagination-button').removeClass('current');
            $button.addClass('current');
            await getGamesAndAppendToHistory(username, i + 1, pageSize);
        })
    }
}

function buildPaginationButton(pageNumber, isCurrent) {
    return $('<a>')
        .addClass('pagination-button')
        .addClass(isCurrent ? 'current' : '')
        .text(pageNumber);
}

async function getGamesAndAppendToHistory(username, pageNumber, pageSize) {
    const $gamesHistory = $('#gamesHistory');
    $gamesHistory.empty();
    const games = await getGames(pageNumber, pageSize);
    for (const game of games) {
        const $gameEl = buildGameElement(username, game);
        $gamesHistory.append($gameEl);
        $gameEl.on('click', e => {
            window.location.assign(`/GamesHistory/Review/${game.gameId}`);
        });
    }
    $gamesHistory.scrollTop = $gamesHistory.scrollHeight;
}

async function getGames(pageNumber, pageSize) {
    const games =  await $.ajax({
        url: '/GamesHistory/GetGames',
        method: 'GET',
        data: {
            pageSize: pageSize,
            pageNumber: pageNumber
        },
        dataType: 'json'
    });
    return games;
}

function buildGameElement(username, game) {
    const isWhite = getIsWhite(username, game);
    
    let colorOfResult;
    if (game.gameResult === GameResult.Draw)
        colorOfResult = 'draw';
    else
        colorOfResult = (isWhite && game.gameResult === GameResult.WhiteWon)
            || (!isWhite && game.gameResult === GameResult.BlackWon) 
            ? 'win'
            : 'lose';
    
    return $('<div>')
        .addClass('game-info-container')
        .addClass(colorOfResult)
        .append(getNamesContainer(game))
        .append(getResultRatingChangeContainer(game))
        .append($('<span>').addClass('games-history-date').text(getPlayedDate(game.playedDate)))
        .append($('<span>').addClass('games-history-time-control').text(`${game.minutes}+${game.increment}`));
}

function getNamesContainer(game) {
    return $('<div>')
        .addClass('names-container')
        .append(
            $('<div>')
                .addClass('name-rating-container')
                .append(
                    $('<span>').addClass('name').text(game.blackPlayerName)
                )
                .append(
                    $('<span>').addClass('rating').text(game.blackPlayerRating)
                )
        )
        .append(
            $('<div>')
                .addClass('name-rating-container')
                .append(
                    $('<span>').addClass('name').text(game.whitePlayerName)
                )
                .append(
                    $('<span>').addClass('rating').text(game.whitePlayerRating)
                )
        );
}

function getResultRatingChangeContainer(game) {
    const whitePlayerRatingDelta = getPlayerRatingDelta(game.whitePlayerRating, game.whitePlayerNewRating);
    const blackPlayerRatingDelta = getPlayerRatingDelta(game.blackPlayerRating, game.blackPlayerNewRating);
    const [whitePoints, blackPoints] = getWhiteAndBlackPoints(game.gameResult);
    return $('<div>')
        .addClass('result-rating-change-container')
        .append(
            $('<div>')
                .addClass('rating-change-container')
                .append($('<span>').text(blackPlayerRatingDelta))
                .append($('<span>').text(whitePlayerRatingDelta))
        )
        .append(
            $('<div>')
                .addClass('result-container')
                .append($('<span>').text(blackPoints))
                .append($('<span>').text(whitePoints))
        )
}

function getPlayedDate(playedDate) {
    return new Date(playedDate)
        .toLocaleString("ru-RU", {
            day:    "2-digit",
            month:  "2-digit",
            year:   "numeric",
            hour12: false,
            timeZone: "UTC"
        });
}

function getPlayerRatingDelta(oldRating, newRating) {
    const delta = newRating - oldRating;
    if (delta > 0)
        return `+${delta}`;
    return delta;
}

function getWhiteAndBlackPoints(gameResult) {
    switch (gameResult) {
        case GameResult.Draw:
            return ['½', '½']
        case GameResult.WhiteWon:
            return ['1', '0']
        case GameResult.BlackWon:
            return ['0', '1']
        default:
            console.error(`Unexpected game result: ${gameResult}`);
            return [NaN, NaN];
    }
}

function getIsWhite(username, game) {
    if (username !== game.whitePlayerName && username !== game.blackPlayerName)
        console.error(
            `Something went wrong with getting 
            color you playd as: non of the game 
            ${game.gameId} names matches with your username.`
        );
    return username === game.whitePlayerName;
}