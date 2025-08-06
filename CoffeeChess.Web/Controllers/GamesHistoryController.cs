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

    public async Task<IActionResult> Review(string gameId, CancellationToken cancellationToken)
    {
        // NOTE: for testing
        gameId = "7ed025c3"; // TODO: remove
        try
        {
            var query = new GetCompletedGameQuery(gameId);
            var game = await mediator.Send(query, cancellationToken);
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_Review", game);
            }

            return View("Review", game);
        }
        catch (Exception ex)
        {
            return NotFound($"Game with ID {gameId} not found.");
        }
    }
}