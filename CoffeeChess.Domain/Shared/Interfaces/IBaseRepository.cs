namespace CoffeeChess.Domain.Shared.Interfaces;

public interface IBaseRepository<T>
{
    public Task<T?> GetByIdAsync(string id);
    public Task AddAsync(T entity);
    public Task DeleteAsync(T entity);
    
    public IAsyncEnumerable<T> GetAllAsync();
    public Task SaveChangesAsync(T entity);
}