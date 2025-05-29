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
        ColorPreferences colorPreference = ColorPreferences.Any,
        int minRating = int.MinValue,
        int maxRating = int.MaxValue)
    {
        // TODO: move to lobby
        throw new NotImplementedException();
    }
}