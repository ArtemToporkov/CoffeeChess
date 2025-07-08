import { GameActionType } from "./GameActionType.js";
import { GameResultForPlayer } from "./GameResultForPlayer.js";

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
    
    $('#promotionPanelCloseButton').on('click', () => $('#promotionPanel').css('display', 'none'));
    
    bindEventsToResignButton(connection, gameId);
    bindEventsToDrawOfferButtons(connection, gameId);
    bindEventsToAnalyzeButtons();
}

export function receiveDrawOffer(connection, message) {
    $('#resignDrawMessage').text(message);
    $('#resignDrawButtonsContainer').css('display', 'none');
    $('#drawOfferContainer').css('display', 'flex');
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

export function updateGameResult(result, message, oldRating, newRating) {
    const [panelColorClass, fontButtonsColorClass] = result === GameResultForPlayer.Lost 
        ? ['dark', 'milk'] 
        : ['milk', 'dark'];

    $('.modal-overlay').addClass('show');
    $('#resultPanel')
        .css('display', 'flex')
        .addClass(panelColorClass);
    $('#resultTitle').text(
        ['You win', 'You lose', 'Draw'][result]
    ).addClass(fontButtonsColorClass);
    $('#resultInfo')
        .text(message)
        .addClass(fontButtonsColorClass);
    $('.result-ratings-title').addClass(fontButtonsColorClass);
    $('.result-rating').addClass(fontButtonsColorClass);
    $('.result-button').addClass(fontButtonsColorClass);
    playRatingsChangeAnimation(oldRating, newRating);
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
        display: 'flex'
    });

    $panel.find('.promotion-piece').one('click', function() {
        const promotionPiece = $(this).data('promotionChar');
        $panel.hide();
        promotionCallback(promotionPiece);
    });
}

function playRatingsChangeAnimation(oldRating, newRating) {
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

function bindEventsToResignButton(connection, gameId) {
    $('#resignButton').on('click', () => {
        $('#resignDrawMessage').text('Are you sure?');
        $('#resignDrawButtonsContainer').css('display', 'none');
        $('#resignConfirmationContainer').css('display', 'flex');
    });
    
    $('#confirmButton').on('click', () => {
        connection.invoke('PerformGameAction', gameId, GameActionType.Resign);
    });

    $('#denyButton').on('click', () => {
        turnButtonsBack();
    });
}

function bindEventsToDrawOfferButtons(connection, gameId) {
    $('#drawOfferButton').on('click', () => {
        $('#drawOfferButton').css({
            'background-color': 'var(--milk-coffee-unselected)',
            'box-shadow': '-2px 2px 0 var(--dark-coffee), -3px 3px 0 var(--milk-coffee-unselected)',
            'pointer-events': 'none'
        });
        
        connection.invoke('PerformGameAction', gameId, GameActionType.SendDrawOffer);
    });
    
    $('#acceptButton').on('click', () => {
        connection.invoke("PerformGameAction", gameId, GameActionType.AcceptDrawOffer);
    });

    $('#declineButton').on('click', () => {
        turnButtonsBack();
        connection.invoke('PerformGameAction', gameId, GameActionType.DeclineDrawOffer);
    });
}

function bindEventsToAnalyzeButtons() {
    const $resultPanel = $('.result-panel');
    $('#analyzeButton').on('click', () => {
       $('.modal-overlay').removeClass('show');
        $resultPanel.addClass('close').one('animationend', () => {
           $resultPanel.removeClass('close');
           $resultPanel.css('display', 'none');
       });
    });
}