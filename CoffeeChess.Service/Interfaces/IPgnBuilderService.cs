using CoffeeChess.Core.Enums;

namespace CoffeeChess.Service.Interfaces;

public interface IPgnBuilderService
{
    public void AppendMove(string moveSan, TimeSpan timeLeft);
    public void AppendDraw();
    public void AppendResult(PlayerColor player);
    public string Get();
}