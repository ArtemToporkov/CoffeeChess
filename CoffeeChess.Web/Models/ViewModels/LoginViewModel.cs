using System.ComponentModel.DataAnnotations;

namespace CoffeeChess.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Login is required")]
    [Display(Name = "Login")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}