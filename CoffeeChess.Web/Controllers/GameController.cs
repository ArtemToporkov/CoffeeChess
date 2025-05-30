using CoffeeChess.Web.Enums;
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
        int minRating = int.MinValue,
        int maxRating = int.MaxValue)
    {
        // TODO: MOVE TO LOBBY
        
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_Game");
        }

        return View("Game");
    }
}