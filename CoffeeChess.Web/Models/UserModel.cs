using Microsoft.AspNetCore.Identity;

namespace CoffeeChess.Web.Models;

public class UserModel : IdentityUser
{
    public int Rating { get; set; } = 1500;
}