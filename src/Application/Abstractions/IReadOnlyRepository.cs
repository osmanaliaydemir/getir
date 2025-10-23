using System.Linq.Expressions;

namespace Getir.Application.Abstractions;

/// <summary>
/// Readonly repository interface
/// </summary>
public interface IReadOnlyRepository<T> where T : class
{
    /// <summary>
    /// Filtreye uygun kayıt var mı kontrol et
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt var mı</returns>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Filtreye uygun kayıt sayısını getir
    /// </summary>
    /// <param name="filter">Filtre expression'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt sayısı</returns>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);
    
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
    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        string? include = null,
        int? take = null,
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
}
