using System.Text;
using CoffeeChess.Domain.Services.Interfaces;

namespace CoffeeChess.Domain.Services.Implementations;

public class StringBuilderPgnBuilderService : IPgnBuilderService
{
    public string GetPgn(IReadOnlyCollection<string> sanMovesHistory)
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