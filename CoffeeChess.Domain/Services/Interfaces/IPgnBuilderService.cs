using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Services.Interfaces;

public interface IPgnBuilderService
{
    public string GetPgn(IReadOnlyCollection<string> sanMovesHistory);
}