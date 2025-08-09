$(document).ready(async () => {
    const pageSize = 10;
    
    const gamesCount = await $.ajax({
        url: '/GamesHistory/GetCount',
        method: 'GET',
        dataType: 'json'
    });
    console.log(gamesCount);
    
    const games = await getGames(1, pageSize);
    console.log(games);
});

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