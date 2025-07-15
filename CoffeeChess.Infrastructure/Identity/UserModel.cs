using Microsoft.AspNetCore.Identity;

namespace CoffeeChess.Infrastructure.Identity;

public class UserModel : IdentityUser
{
    public int Rating { get; set; } = 1500;
}