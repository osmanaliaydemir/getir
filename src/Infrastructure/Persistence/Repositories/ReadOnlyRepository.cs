using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence.Repositories;

public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public ReadOnlyRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(filter, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        string? include = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(include))
        {
            foreach (var includeProperty in include.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> filter,
        string? include = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(include))
        {
            foreach (var includeProperty in include.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        return await query.FirstOrDefaultAsync(filter, cancellationToken);
    }
    
    public virtual async Task<IReadOnlyList<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        int page = 1,
        int pageSize = 20,
        string? include = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(include))
        {
            foreach (var includeProperty in include.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        var skip = (page - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
    }
}
