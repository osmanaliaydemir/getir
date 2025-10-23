namespace Getir.Application.Abstractions;

/// <summary>
/// UnitOfWork interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Generic repository getir
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>Generic repository</returns>
    IGenericRepository<T> Repository<T>() where T : class;
    
    /// <summary>
    /// Read-only repository getir
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>Read-only repository</returns>
    IReadOnlyRepository<T> ReadRepository<T>() where T : class;
    
    /// <summary>
    /// Değişiklikleri kaydet
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction başlat
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction'ı onayla
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);    
    
    /// <summary>
    /// Transaction'ı geri al
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
