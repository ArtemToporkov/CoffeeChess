using CoffeeChess.Core.Enums;
using CoffeeChess.Web.Models;
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

    public IActionResult CreateGame(
        int minutes, int increment,
        ColorPreference colorPreference = ColorPreference.Any,
        int minRating = 0,
        int maxRating = int.MaxValue)
    {
        var gameSettings = new GameSettingsViewModel
        {
            Minutes = minutes,
            Increment = increment,
            ColorPreference = colorPreference,
            MinRating = minRating,
            MaxRating = maxRating
        };
        
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GameWaiting", gameSettings);
        }

        return View("GameWaiting", gameSettings);
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