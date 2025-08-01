namespace CoffeeChess.Domain.Shared.Interfaces;

public interface IBaseRepository<T>
{
    public Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    public Task AddAsync(T entity, CancellationToken cancellationToken = default);
    
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
    public Task SaveChangesAsync(T entity, CancellationToken cancellationToken = default);
}