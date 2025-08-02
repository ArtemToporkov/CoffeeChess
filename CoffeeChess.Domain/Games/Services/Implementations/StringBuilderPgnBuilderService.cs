using System.Text;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Games.Services.Implementations;

public class StringBuilderPgnBuilderService : IPgnBuilderService
{
    public string GetPgn(IReadOnlyCollection<SanMove> sanMovesHistory)
    {
        var sb = new StringBuilder();
        var currentPly = 0;
        foreach (var move in sanMovesHistory)
        {
            currentPly++;
            if (currentPly % 2 == 0)
                sb.Append($"{move}\n");
            else
                sb.Append($"{currentPly / 2 + 1}. {move} ");
        }
        return sb.ToString();
    }
}