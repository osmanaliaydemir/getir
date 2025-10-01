using System.Linq.Expressions;

namespace Getir.Application.Abstractions;

public interface IReadOnlyRepository<T> where T : class
{
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default);
    
    Task<int> CountAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        string? include = null,
        int? take = null,
        CancellationToken cancellationToken = default);
    
    Task<T?> FirstOrDefaultAsync(
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
}
