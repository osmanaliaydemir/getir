using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IStockService
{
    /// <summary>
    /// Stok uyarılarını getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok uyarıları</returns>
    /// </summary>
    Task<List<StockAlertResponse>?> GetStockAlertsAsync(CancellationToken ct = default);

    /// <summary>
    /// Stok özetini getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok özeti</returns>
    /// </summary>
    Task<StockSummaryResponse?> GetStockSummaryAsync(CancellationToken ct = default);

    /// <summary>
    /// Stok geçmişini getirir.
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok geçmişi</returns>
    /// </summary>
    Task<List<StockHistoryResponse>?> GetStockHistoryAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);

    /// <summary>
    /// Stok seviyesini günceller.
    /// </summary>
    /// <param name="request">Stok seviyesi güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    /// </summary>
    Task<bool> UpdateStockLevelAsync(UpdateStockRequest request, CancellationToken ct = default);

    /// <summary>
    /// Stok seviyelerini bulk günceller.
    /// </summary>
    /// <param name="request">Stok seviyeleri bulk güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    /// </summary>
    Task<bool> BulkUpdateStockLevelsAsync(BulkUpdateStockRequest request, CancellationToken ct = default);

    /// <summary>
    /// Stok uyarısını çözümler.
    /// </summary>
    /// <param name="alertId">Stok uyarısı ID</param>
    /// <param name="resolutionNotes">Çözüm notları</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    /// </summary>
    Task<bool> ResolveStockAlertAsync(Guid alertId, string resolutionNotes, CancellationToken ct = default);

    /// <summary>
    /// Stok seviyelerini kontrol eder.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    /// </summary>
    Task<bool> CheckStockLevelsAsync(CancellationToken ct = default);
}

