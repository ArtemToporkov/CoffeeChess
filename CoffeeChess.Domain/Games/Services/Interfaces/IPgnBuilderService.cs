using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Games.Services.Interfaces;

public interface IPgnBuilderService
{
    public string GetPgn(IReadOnlyCollection<SanMove> sanMovesHistory);
}