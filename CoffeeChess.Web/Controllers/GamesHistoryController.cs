using Microsoft.AspNetCore.Mvc;

namespace CoffeeChess.Web.Controllers;

public class GamesHistoryController : Controller
{
    public IActionResult GamesHistory()
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_GamesHistory");
        }

        return View("GamesHistory");
    }
}