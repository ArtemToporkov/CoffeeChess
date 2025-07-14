using System.Text;
using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Service.Interfaces;

namespace CoffeeChess.Service.Implementations;

public class BasePgnBuilderService : IPgnBuilderService
{
    private readonly StringBuilder _pgnBuilder = new();
    private int _pliesCount;
    
    public BasePgnBuilderService(
        DateTime utcDateTime,
        TimeSpan timeControl,
        TimeSpan increment,
        PlayerInfoModel white, 
        PlayerInfoModel black)
    {
        _pgnBuilder.Append($"[UTCDate \"{utcDateTime.ToString("yyyy-MM-dd")}\"]\n")
            .Append($"[UTCEndTime \"{utcDateTime.ToString("hh:mm:ss")}\"]\n")
            .Append($"[White \"{white.Name}\"]\n")
            .Append($"[Black \"{black.Name}\"]\n")
            .Append($"[Result \"?\"]")
            .Append($"[WhiteElo \"{white.Rating}\"]\n")
            .Append($"[BlackElo \"{black.Rating}\"]\n")
            .Append($"[TimeControl \"{timeControl.TotalSeconds}+{increment}\"]\n");
    }
    
    public void AppendMove(string moveSan, TimeSpan timeLeft)
    {
        var minutesLeft = timeLeft.Minutes < 10 ? "00" : $"{timeLeft.Minutes}";
        var secondsLeft = timeLeft.Seconds < 10 ? "00" : $"{timeLeft.Seconds}";
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