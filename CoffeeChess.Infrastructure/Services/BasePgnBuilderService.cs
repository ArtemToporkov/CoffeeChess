using System.Text;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Interfaces;

namespace CoffeeChess.Infrastructure.Services;

public class BasePgnBuilderService : IPgnBuilderService
{
    private readonly StringBuilder _pgnBuilder = new();
    private int _pliesCount;
    
    public BasePgnBuilderService(
        DateTime utcDateTime,
        TimeSpan timeControl,
        TimeSpan increment,
        PlayerInfo white, 
        PlayerInfo black)
    {
        _pgnBuilder.Append($"[UTCDate \"{utcDateTime:yyyy-MM-dd)}\"]\n")
            .Append($"[UTCEndTime \"{utcDateTime:hh:mm:ss}\"]\n")
            .Append($"[White \"{white.Name}\"]\n")
            .Append($"[Black \"{black.Name}\"]\n")
            .Append($"[Result \"?\"]\n")
            .Append($"[WhiteElo \"{white.Rating}\"]\n")
            .Append($"[BlackElo \"{black.Rating}\"]\n")
            .Append($"[TimeControl \"{(int)timeControl.TotalSeconds}+{(int)increment.TotalSeconds}\"]\n");
    }
    
    public void AppendMove(string moveSan, TimeSpan timeLeft)
    {
        var minutesLeft = timeLeft.Minutes.ToString("D2");
        var secondsLeft = timeLeft.Seconds.ToString("D2");
        var lastMoveWasBlack = _pliesCount % 2 == 0;
        _pliesCount++;
        var (lineBreak, moveNumber) = lastMoveWasBlack
            ? (string.Empty, $"{Math.Ceiling(_pliesCount / 2.0)}.")
            : ("\n", string.Empty);
        var move = $"{moveNumber} {moveSan} {{[%clk {minutesLeft}:{secondsLeft}]}}{lineBreak}";
        _pgnBuilder.Append(move);
    }

    public void AppendDraw() => AppendResult("1/2-1/2");

    public void AppendResult(PlayerColor player) => AppendResult(player is PlayerColor.White ? "1-0" : "0-1");

    public string Get() => _pgnBuilder.ToString();

    private void AppendResult(string result)
    {
        _pgnBuilder.Length--;
        _pgnBuilder.Append($" {result}");
        _pgnBuilder.Replace("\"?\"", $"\"{result}\"");
    }
}