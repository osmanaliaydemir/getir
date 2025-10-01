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

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<Type, object>();
        _readRepositories = new ConcurrentDictionary<Type, object>();
    }

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

    public IReadOnlyRepository<T> ReadRepository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_readRepositories.ContainsKey(type))
        {
            var repositoryInstance = new ReadOnlyRepository<T>(_context);
            _readRepositories.TryAdd(type, repositoryInstance);
        }
        
        return (IReadOnlyRepository<T>)_readRepositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
