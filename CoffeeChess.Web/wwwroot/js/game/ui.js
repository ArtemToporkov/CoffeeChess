export function loadUi(gameManager) {
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
    
    const $resignDrawButtonsContainer = $('#resignDrawButtonsContainer');
    const $confirmButtonsContainer = $('#confirmationButtonsContainer');
    const $confirmButton = $('#confirmButton');
    const $denyButton = $('#denyButton');
    const $resignDrawMessage = $('#resignDrawMessage');
    
    const turnButtonsBack = () => {
        $resignDrawMessage.text('');
        $resignDrawButtonsContainer.css('display', 'flex');
        $confirmButtonsContainer.css('display', 'none');
    };
    
    $('#resignButton').on('click', () => {
        $resignDrawMessage.text('Are you sure?');
        $resignDrawButtonsContainer.css('display', 'none');
        $confirmButtonsContainer.css('display', 'flex');
        
        $confirmButton.text('Yes');
        $confirmButton.on('click', () => {
            // TODO: implement resignation
        });
        
        $denyButton.text('No');
        $denyButton.on('click', () => {
            turnButtonsBack();
        });
    })
}