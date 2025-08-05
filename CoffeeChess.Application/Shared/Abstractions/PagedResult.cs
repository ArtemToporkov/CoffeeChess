namespace CoffeeChess.Application.Shared.Abstractions;

public class PagedResult<T> where T : class
{
    public IReadOnlyList<T> Items { get; init; } = null!;
    public int Page { get; init; }
    public int PageSize { get; init; }
}