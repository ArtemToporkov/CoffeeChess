import { GameActionType } from "./enums/GameActionType.js";
import { ButtonStyle } from "./enums/ButtonStyle.js";
import { GameResult } from "./enums/GameResult.js";
import { GameHubMethods } from "./enums/GameHubMethods.js";

export function loadUi(connection, gameManager, gameId) {
    const whitePlayerInfo = JSON.parse(localStorage.getItem('whitePlayerInfo'));
    const blackPlayerInfo = JSON.parse(localStorage.getItem('blackPlayerInfo'));

    $('#whiteUsername').text(whitePlayerInfo.name);
    $('#blackUsername').text(blackPlayerInfo.name);
    $('#whiteRating').text(whitePlayerInfo.rating);
    $('#blackRating').text(blackPlayerInfo.rating);

    if (!gameManager.isWhite) {
        gameManager.board.flip();
        $('.game-middle-panel').addClass('flipped');
    }
    
    $('#promotionPanelCloseButton').on('click', hidePromotionModal);
    
    bindEventsToResignButton(connection, gameId);
    bindEventsToDrawOfferButtons(connection, gameId);
    $('#analyzeButton').on('click', closeResultPanel);
}

export function receiveDrawOffer(connection, message) {
    $('#resignDrawMessage').text(message);
    $('#resignDrawButtonsContainer').css('display', 'none');
    $('#drawOfferContainer').css('display', 'flex');
}

export function setDrawOfferInactive() {
    $('#drawOfferButton').css(ButtonStyle.Inactive);
}

export function setDrawOfferActive() {
    $('#drawOfferButton').css(ButtonStyle.Active);
}

export function turnButtonsBack() {
    $('#resignDrawMessage').text('');
    $('#resignDrawButtonsContainer').css('display', 'flex');
    $('#resignConfirmationContainer').css('display', 'none');
    $('#drawOfferContainer').css('display', 'none');
    $('#drawOfferButton').css({});
}

export function turnOffDrawResignInfo() {
    $('.resign-draw-info').css('display', 'none');
}

export function showResultModal(isWhite, result, reason) {
    const [panelColorClass, fontButtonsColorClass] 
        = (isWhite && result === GameResult.BlackWon) || (!isWhite && result === GameResult.WhiteWon)
        ? ['dark', 'milk'] 
        : ['milk', 'dark'];

    $('#modalOverlay').addClass('show');
    $('#resultPanel')
        .css('display', 'flex')
        .addClass(panelColorClass);
    
    $('#resultTitle').text(
        getResultTitle(isWhite, result)
    ).addClass(fontButtonsColorClass);
    $('#resultInfo')
        .text(reason)
        .addClass(fontButtonsColorClass);
    $('.result-info-title').addClass(fontButtonsColorClass);
    $('.result-info').addClass(fontButtonsColorClass);
    $('.result-button').addClass(fontButtonsColorClass);
}

export function highlightSquares(from, to) {
    unhighlightSquares();
    $(`.square-${from}`).addClass('highlighted-square');
    $(`.square-${to}`).addClass('highlighted-square');
}

export function unhighlightSquares() {
    $('.highlighted-square').removeClass('highlighted-square');
}

export function showPromotionDialog(square, isWhite, promotionCallback) {
    const $panel = $('#promotionPanel');
    $panel.find(`.promotion-piece.${isWhite ? "white-piece" : "black-piece"}`)
        .show();
    
    const squareOffset = $(`.square-${square}`).offset();
    if (squareOffset === undefined)
        return;
    
    $panel.css({
        top: `${squareOffset.top}px`,
        left: `${squareOffset.left}px`,
        display: 'grid'
    });
    
    $('#modalOverlay').addClass('show');
    $panel.addClass('show');

    $panel.find('.promotion-piece')
        .off('click')
        .one('click', function() {
        const promotionPiece = $(this).data('promotionChar');
        hidePromotionModal();
        promotionCallback(promotionPiece);
    });
}

export function animateSearching() {
    const $title = $('#waitingTitle');
    const widths = [];
    $title.text("Searching.");
    widths.push($title.width());
    $title.text("Searching..");
    widths.push($title.width());
    $title.text("Searching...");
    widths.push($title.width());
    $title.text("Searching.");
    
    let idx = 0;
    setInterval(() => {
        $title.text(`Searching${'.'.repeat(idx + 1)}`);
        $title.css('width', widths[idx]);
        idx = (idx + 1) % 3;
    }, 600);
}

export function playRatingsChangeAnimation(oldRating, newRating) {
    const $oldRating = $('#oldRating');
    const $newRating = $('#newRating');
    const $ratingDelta = $('#ratingDelta');

    $oldRating.text(oldRating);
    const delta = newRating - oldRating;
    $ratingDelta.text(delta < 0 ? delta : `+${delta}`)
    const delay = 400;
    
    setTimeout(() => {
       $oldRating.addClass('show'); 
    }, delay);

    setTimeout(() => {
        $oldRating.addClass('strike');
    }, delay * 2);

    setTimeout(() => {
        $ratingDelta.addClass('show');
    }, delay * 3);

    setTimeout(() => {
        $newRating.addClass('show');
        let current = oldRating;
        const duration = 800;
        const steps = duration / 16;
        const step = (newRating - oldRating) / steps;
        const timer = setInterval(() => {
            current += step;
            if ((step > 0 && current >= newRating) || (step < 0 && current <= newRating)) {
                current = newRating;
                clearInterval(timer);
            }
            $newRating.text(Math.round(current));
        }, 16);
    }, delay * 4);
}

export function closeResultPanel() {
    const $resultPanel = $('.result-panel');
    $('#modalOverlay').removeClass('show');
    $resultPanel.addClass('close').one('animationend', () => {
        $resultPanel.removeClass('close');
        $resultPanel.css('display', 'none');
    });
}

export function setTimerHighlighting(toWhite) {
    const $whiteTimer = $('#whiteTimeLeft').parent();
    const $blackTimer = $('#blackTimeLeft').parent();
    const [toSetActive, toSetInactive] = toWhite
        ? [$whiteTimer, $blackTimer]
        : [$blackTimer, $whiteTimer];
    toSetActive.removeClass('inactive');
    toSetInactive.addClass('inactive');
}

export function setResultPoints(result) {
    const $whitePoints = $('#whitePoints');
    const $blackPoints = $('#blackPoints');
    switch (result) {
        case GameResult.WhiteWon:
            $whitePoints.text('1');
            $blackPoints.text('0');
            break;
        case GameResult.BlackWon:
            $whitePoints.text('0');
            $blackPoints.text('1');
            break;
        case GameResult.Draw:
            $whitePoints.text('½');
            $blackPoints.text('½');
            break;
    }
    unhideResultPoints($whitePoints.parent(), $whitePoints.parent().parent());
    unhideResultPoints($blackPoints.parent(), $blackPoints.parent().parent());
}

function unhideResultPoints($pointsContainer, $playerInfoContainer) {
    const width = $playerInfoContainer.outerWidth(true);
    $playerInfoContainer.width(width);
    const widthWithPoints = width
        + $pointsContainer.outerWidth(true) 
        + parseInt($playerInfoContainer.css('gap'));
    $playerInfoContainer.width(widthWithPoints).one('transitionend', () => {
        $pointsContainer.removeClass('hide');
    });
}

function getResultTitle(isWhite, result) {
    if (result === GameResult.Draw)
        return 'Draw';
    if (isWhite && result === GameResult.WhiteWon 
        || !isWhite && result === GameResult.BlackWon)
        return "You win";
    return "You lose";
}

function hidePromotionModal() {
    $('#modalOverlay').removeClass('show');
    $('#promotionPanel').removeClass('show');
}

function bindEventsToResignButton(connection, gameId) {
    $('#resignButton').on('click', () => {
        setResultPoints(GameResult.Draw);
        $('#resignDrawMessage').text('Are you sure?');
        $('#resignDrawButtonsContainer').css('display', 'none');
        $('#resignConfirmationContainer').css('display', 'flex');
    });
    
    $('#confirmButton').on('click', () => {
        connection.invoke(GameHubMethods.PerformGameAction, gameId, GameActionType.Resign);
    });

    $('#denyButton').on('click', () => {
        turnButtonsBack();
    });
}

function bindEventsToDrawOfferButtons(connection, gameId) {
    $('#drawOfferButton').on('click', () => {
        connection.invoke(GameHubMethods.PerformGameAction, gameId, GameActionType.SendDrawOffer);
    });
    
    $('#acceptButton').on('click', () => {
        connection.invoke(GameHubMethods.PerformGameAction, gameId, GameActionType.AcceptDrawOffer);
    });

    $('#declineButton').on('click', () => {
        connection.invoke(GameHubMethods.PerformGameAction, gameId, GameActionType.DeclineDrawOffer);
    });
}