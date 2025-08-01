using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoffeeChess.Web.Models;
using CoffeeChess.Web.Models.ViewModels;

namespace CoffeeChess.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}