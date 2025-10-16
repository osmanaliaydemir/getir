using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Getir.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence.Repositories;

/// <summary>
/// Optimized repository for complex queries with proper includes to prevent N+1 problems
/// </summary>
public class OptimizedRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public OptimizedRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? include = null, CancellationToken cancellationToken = default)
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

    public virtual async Task<IReadOnlyList<T>> GetPagedAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, int page = 1, int pageSize = 20, string? include = null, CancellationToken cancellationToken = default)
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

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, string? include = null, CancellationToken cancellationToken = default)
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

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(filter, cancellationToken);
    }

    // Optimized methods for specific use cases

    /// <summary>
    /// Get entities with multiple includes to prevent N+1 problems
    /// </summary>
    public async Task<IReadOnlyList<T>> GetWithIncludesAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null,
        int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get entities with split query to handle complex includes
    /// </summary>
    public async Task<IReadOnlyList<T>> GetWithSplitQueryAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking().AsSplitQuery();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync(cancellationToken);
    }

    // CRUD operations (for write operations, don't use AsNoTracking)
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    // Missing interface methods
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? include = null, CancellationToken cancellationToken = default)
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

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, int? limit = null, string? include = null, CancellationToken cancellationToken = default)
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

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
