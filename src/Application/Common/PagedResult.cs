namespace Getir.Application.Common;

/// <summary>
/// Sayfalanmış sonuç verisi için generic sınıf
/// </summary>
/// <typeparam name="T">Veri tipi</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Sayfa verileri
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Mevcut sayfa numarası
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Sayfa boyutu
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Toplam sayfa sayısı
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    
    /// <summary>
    /// Önceki sayfa var mı
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// Sonraki sayfa var mı
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    
    /// <summary>
    /// PagedResult instance oluştur
    /// </summary>
    /// <param name="items">Sayfa verileri</param>
    /// <param name="total">Toplam kayıt sayısı</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <returns>PagedResult instance</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int total, int page, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
