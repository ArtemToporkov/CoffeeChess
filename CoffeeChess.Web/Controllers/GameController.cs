using System.Security.Authentication;
using System.Security.Claims;
using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Application.Matchmaking.Commands;
using CoffeeChess.Web.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class GameController(IMediator mediator) : Controller
{
    public IActionResult GameCreation()
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GameCreation");
        }

        return View("GameCreation");
    }

    public IActionResult GameCreationExtended()
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GameCreationExtended");
        }

        return View("GameCreationExtended");
    }

    public IActionResult CreateGame(ChallengeSettingsViewModel settings)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GameWaiting", settings);
        }

        return View("GameWaiting", settings);
    }

    [HttpPost("Game/GameCreation/QueueOrFindChallenge")]
    public async Task<IActionResult> QueueOrFindChallenge([FromBody] ChallengeSettingsViewModel settings)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();
        
        await mediator.Send(new QueueOrFindChallengeCommand(
            userId, 
            settings.Minutes, 
            settings.Increment, 
            settings.ColorPreference, 
            settings.MinRating, 
            settings.MaxRating));
        return Ok();
    }
    
    public IActionResult Play(string gameId)
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_Game");
        }

        return View("Game");
    }

    [HttpGet("/Game/GetGameInfo/{gameId}")]
    public async Task<IActionResult> GetGameInfo(string gameId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();
        var getGameInfoCommand = new GetPlayerGameInfoCommand(gameId, userId);
        return Json(await mediator.Send(getGameInfoCommand));
    }
}