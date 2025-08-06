($(document).ready(async () => {
    const pathParts = window.location.pathname.split('/');
    const gameId = pathParts[pathParts.length - 1];
    const game = await $.ajax({
        url: `/GamesHistory/GetGame/${gameId}`,
        dataType: 'json'
    });
    const username = $('#username').text();
    if (!username) {
        document.title = `${game.whitePlayerName} vs. ${game.blackPlayerName} - CoffeeChess`;
    } else if (username === game.whitePlayerName) {
        document.title = `Review vs. ${game.blackPlayerName} - CoffeeChess`
    } else {
        document.title = `Review vs. ${game.whitePlayerName} - CoffeeChess`
    }
    console.log(game);
}));