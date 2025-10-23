using System.Collections.Concurrent;
using Getir.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Getir.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private readonly ConcurrentDictionary<Type, object> _readRepositories;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// UnitOfWork constructor
    /// </summary>
    /// <param name="context">Entity Framework context</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "AppDbContext cannot be null in UnitOfWork. Ensure DbContext is properly registered in DI container.");
        _repositories = new ConcurrentDictionary<Type, object>();
        _readRepositories = new ConcurrentDictionary<Type, object>();
    }
    /// <summary>
    /// Generic repository getir
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>Generic repository</returns>
    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.TryAdd(type, repositoryInstance);
        }
        
        return (IGenericRepository<T>)_repositories[type];
    }

    /// <summary>
    /// Read-only repository getir
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>Read-only repository</returns>
    public IReadOnlyRepository<T> ReadRepository<T>() where T : class
    {
        if (_context == null)
        {
            throw new InvalidOperationException($"AppDbContext is null when creating ReadRepository<{typeof(T).Name}>. Check DI registration.");
        }
        
        var type = typeof(T);
        
        if (!_readRepositories.ContainsKey(type))
        {
            var repositoryInstance = new ReadOnlyRepository<T>(_context);
            _readRepositories.TryAdd(type, repositoryInstance);
        }
        
        return (IReadOnlyRepository<T>)_readRepositories[type];
    }

    /// <summary>
    /// Değişiklikleri kaydet
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Transaction başlat
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Transaction'ı onayla
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Transaction'ı geri al
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Kaynakları serbest bırak
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
