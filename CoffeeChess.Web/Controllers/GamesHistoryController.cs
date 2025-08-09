using System.Security.Authentication;
using System.Security.Claims;
using CoffeeChess.Application.Games.Queries;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class GamesHistoryController(
    IMediator mediator) : Controller
{
    public IActionResult GamesHistory()
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GamesHistory");
        }

        return View("GamesHistory");
    }

    public IActionResult Review(string gameId, CancellationToken cancellationToken)
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_Review");
        }

        return View("Review");
    }

    [HttpGet("/GamesHistory/GetGame/{gameId}")]
    public async Task <IActionResult> GetGame(string gameId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetCompletedGameQuery(gameId);
            var game = await mediator.Send(query, cancellationToken);
            ViewBag.Title = $"Review vs. {game.WhitePlayerName}";
            return Json(game);
        }
        catch (Exception ex)
        {
            return NotFound($"Game with ID {gameId} not found.");
        }
    }

    [HttpGet("/GamesHistory/GetCount")]
    public async Task<ActionResult<int>> GetCompletedGamesCountForPlayer(CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var query = new GetCompletedGamesCountQuery(userId);
        var count = await mediator.Send(query, cancellationToken);
        return Ok(count);
    }

    [HttpGet("/GamesHistory/GetGames")]
    public async Task<IActionResult> GetGames(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var query = new GetCompletedGamesPageQuery(userId, pageNumber, pageSize);
        var games = await mediator.Send(query, cancellationToken);
        return Json(games);

    }
    
    private string GetUserIdOrThrow() => User.FindFirstValue(ClaimTypes.NameIdentifier) 
                                         ?? throw new AuthenticationException("User not authenticated.");
}