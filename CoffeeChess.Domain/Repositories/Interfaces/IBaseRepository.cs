using System.Diagnostics.CodeAnalysis;

namespace CoffeeChess.Domain.Repositories.Interfaces;

public interface IBaseRepository<T>
{
    public bool TryGetValue(string id, [NotNullWhen(true)] out T? value);
    public bool TryAdd(string id, T challenge);
    public bool TryRemove(string id, [NotNullWhen(true)] out T? removedValue);
    public IEnumerable<(string, T)> GetAll();
    public void SaveChanges(T obj);
}