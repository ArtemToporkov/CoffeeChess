using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class GameController : Controller
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

    public IActionResult CreateGame([FromQuery] ChallengeSettingsViewModel settings)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GameWaiting", settings);
        }

        return View("GameWaiting", settings);
    }

    public IActionResult Play(string gameId)
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_Game");
        }

        return View("Game");
    }
}