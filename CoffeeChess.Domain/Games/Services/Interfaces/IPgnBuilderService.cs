using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Games.Services.Interfaces;

public interface IPgnBuilderService
{
    public string GetPgnWithMovesOnly(IReadOnlyCollection<San> sanMovesHistory);
}