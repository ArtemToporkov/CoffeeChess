using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Infrastructure.Services;

namespace CoffeeChess.Tests;

public class BasePgnBuilderServiceTests
{
    [Fact]
    public void Get_DrawResult_ReturnsCorrectPgn()
    {
        var white = new PlayerInfo("xxx", "White", 1980);
        var black = new PlayerInfo("xxx", "Black", 1980);
        var date = DateTime.UtcNow;
        var timeControl = TimeSpan.FromMinutes(3);
        var increment = TimeSpan.FromSeconds(2);
        var builder = new BasePgnBuilderService(
            date, 
            timeControl, 
            increment,
            white,
            black);
        var moves = new[] { "e4", "e5"};
        foreach (var move in moves)
            builder.AppendMove(move, TimeSpan.FromSeconds(180));
        builder.AppendDraw();
        var expected = $"[UTCDate \"{date:yyyy-MM-dd)}\"]\n" +
                 $"[UTCEndTime \"{date:hh:mm:ss}\"]\n" +
                 $"[White \"{white.Name}\"]\n" +
                 $"[Black \"{black.Name}\"]\n" +
                 $"[Result \"1/2-1/2\"]\n" +
                 $"[WhiteElo \"{white.Rating}\"]\n" +
                 $"[BlackElo \"{black.Rating}\"]\n" +
                 $"[TimeControl \"180+2\"]\n" +
                 $"1. e4 {{[%clk 03:00]}} e5 {{[%clk 03:00]}} 1/2-1/2";
        
        Assert.Equal(expected, builder.Get());
    }
}