using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// GenericRepository constructor
    /// </summary>
    /// <param name="context">Entity Framework context</param>
    public GenericRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "AppDbContext cannot be null in GenericRepository");
        _dbSet = _context.Set<T>();
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
        IQueryable<T> query = _dbSet.AsNoTracking(); // Performance optimization

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
        IQueryable<T> query = _dbSet.AsNoTracking(); // Performance optimization

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
    /// Entity sil (Delete alias)
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Entity'yi asenkron olarak sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => _dbSet.Remove(entity), cancellationToken);
    }

    /// <summary>
    /// Entity listesi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="orderBy">Sıralama expression'ı</param>
    /// <param name="ascending">Artan sıralama mı</param>
    /// <param name="take">Alınacak kayıt sayısı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, int? take = null, string? include = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

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

    /// <summary>
    /// İlk eşleşen entity'yi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İlk eşleşen entity veya null</returns>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? include = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (!string.IsNullOrEmpty(include))
        {
            var includes = include.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var inc in includes)
            {
                query = query.Include(inc.Trim());
            }
        }

        return await query.FirstOrDefaultAsync(filter, cancellationToken);
    }
}
