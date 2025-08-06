using CoffeeChess.Application.Games.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class GamesHistoryController(IMediator mediator) : Controller
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
}