namespace CoffeeChess.Domain.Games.Services.Interfaces;

public interface IPgnBuilderService
{
    public string GetPgn(IReadOnlyCollection<string> sanMovesHistory);
}