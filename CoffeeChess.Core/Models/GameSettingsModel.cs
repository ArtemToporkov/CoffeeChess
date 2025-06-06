﻿using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models;

public class GameSettingsModel
{
    public int Minutes { get; set; }
    public int Increment { get; set; }
    public ColorPreference ColorPreference { get; set; } = ColorPreference.Any;
    public int MinRating { get; set; } = 0;
    public int MaxRating { get; set; } = int.MaxValue;
}