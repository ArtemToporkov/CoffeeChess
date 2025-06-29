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
    
    bindEventsToResignButton(connection, gameId);
    bindEventsToDrawOfferButtons(connection, gameId);
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

export function updateGameResult(result, message) {
    console.log(result);
    switch (result) {
        case GameResultForPlayer.Draw:
        case GameResultForPlayer.Won:
            $('.modal-overlay').css('display', 'flex');
            $('#milkResultPanel').css('display', 'flex');
            $('#darkResultTitle').text(
                result === GameResultForPlayer.Draw 
                    ? "Draw" 
                    : "You win!"
            );
            break;
        case GameResultForPlayer.Lost:
            $('.modal-overlay').css('display', 'flex');
            $('#darkResultPanel').css('display', 'flex');
            $('#milkResultTitle').text("You lose...");
            break;
    }
}

function bindEventsToResignButton(connection, gameId) {
    $('#resignButton').on('click', () => {
        $('#resignDrawMessage').text('Are you sure?');
        $('#resignDrawButtonsContainer').css('display', 'none');
        $('#resignConfirmationContainer').css('display', 'flex');
    });
    
    $('#confirmButton').on('click', () => {
        // TODO: implement resignation
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
        // TODO: implement draw agreement
    });

    $('#declineButton').on('click', () => {
        turnButtonsBack();
        connection.invoke('PerformGameAction', gameId, GameActionType.DeclineDrawOffer);
    });
}