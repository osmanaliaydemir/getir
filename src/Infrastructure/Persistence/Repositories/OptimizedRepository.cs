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

    /// <summary>
    /// ID'ye göre entity getir
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Filtreye göre entity getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
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

    /// <summary>
    /// Sayfalanmış entity listesi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="orderBy">Sıralama expression'ı</param>
    /// <param name="ascending">Artan sıralama mı</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış entity listesi</returns>
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

    /// <summary>
    /// Tüm entity listesi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="orderBy">Sıralama expression'ı</param>
    /// <param name="ascending">Artan sıralama mı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tüm entity listesi</returns>
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

    /// <summary>
    /// Entity sayısını getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity sayısı</returns>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Entity var mı kontrolü
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity var mı</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Include edilecek navigation property'leri getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="includes">Include edilecek navigation property'ler</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
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
    /// Include edilecek navigation property'leri getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="includes">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
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

    /// <summary>
    /// Entity ekle
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eklenen entity</returns>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Entity güncelle
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="id">Silinecek entity ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// Birden fazla entity ekle
    /// </summary>
    /// <param name="entities">Eklenecek entity'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Birden fazla entity sil
    /// </summary>
    /// <param name="entities">Silinecek entity'ler</param>
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Entity getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity</returns>
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

    /// <summary>
    /// Entity listesi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="orderBy">Sıralama expression'ı</param>
    /// <param name="ascending">Artan sıralama mı</param>
    /// <param name="limit">Alınacak kayıt sayısı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
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
