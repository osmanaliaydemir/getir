using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence.Repositories;

public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// ReadOnlyRepository constructor
    /// </summary>
    /// <param name="context">Entity Framework context</param>
    public ReadOnlyRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "AppDbContext cannot be null in ReadOnlyRepository");

        try
        {
            _dbSet = _context.Set<T>();

            if (_dbSet == null)
            {
                throw new InvalidOperationException($"DbSet<{typeof(T).Name}> is null. Entity may not be configured in AppDbContext.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create DbSet<{typeof(T).Name}>. " +
                $"Inner exception: {ex.Message}. " +
                $"Check that: 1) Database connection is valid, 2) Entity is configured in OnModelCreating, 3) Migrations are applied.",
                ex);
        }
    }

    /// <summary>
    /// Filtreye uygun kayıt var mı kontrol et
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt var mı</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Kayıt sayısını getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt sayısı</returns>
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
    /// Entity listesi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="orderBy">Sıralama expression'ı</param>
    /// <param name="ascending">Artan sıralama mı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="take">Alınacak kayıt sayısı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, string? include = null, int? take = null, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// İlk eşleşen entity'yi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İlk eşleşen entity veya null</returns>
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

        var skip = (page - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
    }
}
