using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Services.Interfaces;

public interface IPgnBuilderService
{
    public void AppendMove(string moveSan, TimeSpan timeLeft);
    public void AppendDraw();
    public void AppendResult(PlayerColor player);
    public string Get();
}