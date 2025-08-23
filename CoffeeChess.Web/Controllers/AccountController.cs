using CoffeeChess.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Web.Models.ViewModels;

namespace CoffeeChess.Web.Controllers
{
    public class AccountController(
        UserManager<UserModel> userManager,
        SignInManager<UserModel> signInManager,
        ILogger<AccountController> logger)
        : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_RegisterPanel");
            }

            return View("RegisterPanel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new UserModel { UserName = model.UserName, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password");

                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation("User signed in after registration.");
                    
                    if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true, 
                            redirectUrl = returnUrl ?? Url.Action("Index", "Home"),
                            username = user.UserName
                        });
                    }
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_RegisterPanel", model); 
            }
            return View("RegisterPanel", model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_LoginPanel");
            }
            return View("LoginPanel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");
                    if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true, 
                            redirectUrl = returnUrl ?? Url.Action("Index", "Home"),
                            username = model.UserName
                        });
                    }
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                 return PartialView("_LoginPanel", model);
            }
            return View("LoginPanel", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}