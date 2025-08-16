import { GameResult } from "../enums/GameResult.js";
import { ajaxNavigator } from "../../site.js";

const init = async () => {
    const pageSize = 10;
    const maxPaginationButtons = 7;

    const username = $('#username').text();
    if (!username) {
        $('#errorMessage').text("You are not authenticated.");
        return;
    }
    
    const gamesCount = await $.ajax({
        url: '/GamesHistory/GetCount',
        method: 'GET',
        dataType: 'json'
    });
    
    if (gamesCount === 0) {
        $('#errorMessage').text("You haven't played any games yet.");
        return;
    }
    
    const pagesCount = Math.ceil(gamesCount / pageSize);
    if (pagesCount > 1) {
        buildPaginationPanel(pagesCount, maxPaginationButtons);
        bindPaginationButtonsEvents(username, pagesCount, pageSize, maxPaginationButtons);
    }
    await getGamesAndAppendToHistory(username, 1, pageSize);
};

async function getGamesAndAppendToHistory(username, pageNumber, pageSize) {
    const games = await getGames(pageNumber, pageSize);
    const gamesElements = [];
    for (const game of games) {
        const $gameEl = buildGameElement(username, game).addClass('hide');
        gamesElements.push($gameEl);
        $gameEl.on('click', async e => {
            await ajaxNavigator.loadContent(`/GamesHistory/Review/${game.gameId}`);
        });
    }

    const $gamesHistory = $('#gamesHistory');
    $gamesHistory.empty();
    gamesElements.forEach($gameEl => $gamesHistory.append($gameEl));
    $gamesHistory.scrollTop(0);
    
    const delay = 100;
    $gamesHistory.find('.game-info-container').each((i, gameInfo) => {
        setTimeout(() => {
            $(gameInfo).removeClass('hide');
        }, (i + 1) * delay);
    });
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

function buildPaginationPanel(totalPages, maxButtons) {
    const innerPagesInfo = getBetweenPageNumbersForPagination(1, totalPages, maxButtons - 2);
    setEllipsis(0, innerPagesInfo.leftEllipsis);
    setEllipsis(1, innerPagesInfo.rightEllipsis);
    const $between = $('#innerPaginationButtons');
    for (const pageNumber of innerPagesInfo.pageNumbers) {
        $between.append($('<a>').addClass('pagination-button').text(pageNumber));
    }
    $('#paginationPanel').append($('<a>').addClass('pagination-button').text(totalPages));
}

function reconstructPaginationButtons(toBeCurrentPageNumber, totalPages, maxButtons) {
    const innerPagesInfo = getBetweenPageNumbersForPagination(toBeCurrentPageNumber, totalPages, maxButtons - 2);
    setEllipsis(0, innerPagesInfo.leftEllipsis);
    setEllipsis(1, innerPagesInfo.rightEllipsis);
    const $between = $('#innerPaginationButtons');
    $between.children().each((i, el) => {
        const $el = $(el);
        const pageNumber = innerPagesInfo.pageNumbers[i];
        $el.text(innerPagesInfo.pageNumbers[i]);
        $el.toggleClass('current', pageNumber === toBeCurrentPageNumber)
    });
    const panelChildren = $('#paginationPanel').children();
    panelChildren.eq(0).toggleClass('current', 1 === toBeCurrentPageNumber);
    panelChildren.eq(panelChildren.length - 1).toggleClass('current', totalPages === toBeCurrentPageNumber);
}

function bindPaginationButtonsEvents(username, totalPages, pageSize, maxButtons) {
    const $panel = $('#paginationPanel');
    const panelChildren = $panel.children();
    const toBind = [panelChildren.eq(0)]
    $('#innerPaginationButtons').children().each((i, el) => toBind.push($(el)));
    const $last = panelChildren.eq(panelChildren.length - 1);
    if ($last.hasClass('pagination-button'))
        toBind.push($last);
    
    toBind.forEach($el => {
        $el.off('click').on('click', async () => {
            const pageNumber = parseInt($el.text());
            reconstructPaginationButtons(pageNumber, totalPages, maxButtons);
            await getGamesAndAppendToHistory(username, pageNumber, pageSize);
        })
    })
}

function setEllipsis(ellipsisNumber, shouldSet) {
    const $panel = $('#paginationPanel');
    const $ellipsis = $panel.find('.ellipsis').eq(ellipsisNumber);
    $ellipsis.text(shouldSet ? '…' : '');
    $ellipsis.toggleClass('hide', !shouldSet);
}

function getBetweenPageNumbersForPagination(current, totalPages, maxInnerCount) {
    if (totalPages - 2 <= maxInnerCount) {
        const result = [];
        for (let i = 2; i <= totalPages - 1; i++) {
            result.push(i);
        }
        return {
            leftEllipsis: false,
            rightEllipsis: false,
            pageNumbers: result
        };
    }

    let start = current - Math.floor(maxInnerCount / 2);
    let end = start + maxInnerCount - 1;

    if (start < 2) {
        start = 2;
        end = start + maxInnerCount - 1;
    }
    if (end > totalPages - 1) {
        end = totalPages - 1;
        start = end - maxInnerCount + 1;
    }
    let leftEllipsis = false
    const result = [];
    if (start > 2) 
        leftEllipsis = true;
    for (let i = start; i <= end; i++) 
        result.push(i);
    let rightEllipsis = false;
    if (end < totalPages - 1) 
        rightEllipsis = true;

    return {
        leftEllipsis: leftEllipsis,
        rightEllipsis: rightEllipsis,
        pageNumbers: result
    };
}

const destroy = async () => {
    
}

export default { init, destroy };