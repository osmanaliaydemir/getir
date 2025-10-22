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

    /// <summary>
    /// Düşük stok ürünlerini getirir.
    /// </summary>
    Task<List<LowStockProductModel>> GetLowStockProductsAsync(Guid merchantId, int threshold = 10, CancellationToken ct = default);

    /// <summary>
    /// CSV'den stok import eder.
    /// </summary>
    Task<StockImportResult> ImportStockFromCsvAsync(Guid merchantId, Stream csvStream, CancellationToken ct = default);

    /// <summary>
    /// Stokları CSV'ye export eder.
    /// </summary>
    Task<byte[]> ExportStockToCsvAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>
    /// Stok raporu oluşturur (backend report endpoint'i üzerinden).
    /// </summary>
    Task<StockReportResponse?> GetStockReportAsync(StockReportRequest request, CancellationToken ct = default);

    /// <summary>
    /// Stok senkronizasyonu başlatır (external systems) – admin/owner yetkisi gerekir.
    /// </summary>
    Task<bool> SynchronizeStockAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>
    /// Stok uyarılarını kontrol eder ve oluşturur (low/out/overstock) – backend check-alerts akışı.
    /// </summary>
    Task<bool> CheckStockAlertsAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>
    /// Reorder point ayarlarını getirir.
    /// </summary>
    Task<List<ReorderPointModel>> GetReorderPointsAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>
    /// Reorder point ayarını kaydeder.
    /// </summary>
    Task<bool> SetReorderPointAsync(Guid productId, int minStock, int maxStock, CancellationToken ct = default);
}

