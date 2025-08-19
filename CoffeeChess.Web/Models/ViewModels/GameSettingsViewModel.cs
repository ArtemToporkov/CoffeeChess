using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Matchmaking.Enums;

namespace CoffeeChess.Web.Models.ViewModels;

public class GameSettingsViewModel
{
    public int Minutes { get; set; }
    public int Increment { get; set; }
    public ColorPreference ColorPreference { get; set; } = ColorPreference.Any;
    public int MinRating { get; set; } = 0;
    public int MaxRating { get; set; } = int.MaxValue;
}