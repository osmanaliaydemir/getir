using System.Linq.Expressions;

namespace Getir.Application.Abstractions;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<T?> GetAsync(
        Expression<Func<T, bool>> filter,
        string? include = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        int page = 1,
        int pageSize = 20,
        string? include = null,
        CancellationToken cancellationToken = default);
    
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    void Update(T entity);
    
    void Remove(T entity);
    
    void RemoveRange(IEnumerable<T> entities);
}
