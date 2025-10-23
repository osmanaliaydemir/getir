using System.Linq.Expressions;

namespace Getir.Application.Abstractions;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// ID'ye göre entity getir
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Filtreye göre entity getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    Task<T?> GetAsync(
        Expression<Func<T, bool>> filter,
        string? include = null,
        CancellationToken cancellationToken = default);
    
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
    Task<IReadOnlyList<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        int page = 1,
        int pageSize = 20,
        string? include = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Entity ekle
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eklenen entity</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Birden fazla entity ekle
    /// </summary>
    /// <param name="entities">Eklenecek entity'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Entity güncelle
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    void Update(T entity);
    
    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    void Remove(T entity);
    
    /// <summary>
    /// Birden fazla entity sil
    /// </summary>
    /// <param name="entities">Silinecek entity'ler</param>
    void RemoveRange(IEnumerable<T> entities);
    
    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    void Delete(T entity);
    
    /// <summary>
    /// Entity sil
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
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
        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            int? take = null,
            string? include = null,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// İlk eşleşen entity'yi getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="include">Include edilecek navigation property'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İlk eşleşen entity veya null</returns>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> filter,
            string? include = null,
            CancellationToken cancellationToken = default);
}
