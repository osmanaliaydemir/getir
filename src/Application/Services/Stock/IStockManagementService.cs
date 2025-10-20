using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stok yönetim servisi: sipariş bazlı otomatik stok düşürme/iade, geçmiş, uyarılar, manuel güncelleme, raporlar, senkronizasyon.
/// </summary>
public interface IStockManagementService
{
    /// <summary>Sipariş onaylandığında otomatik stok düşürür (transaction, stok geçmişi, uyarı kontrolü).</summary>
    Task<Result> ReduceStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş iptal edildiğinde stok iade eder (transaction, stok geçmişi).</summary>
    Task<Result> RestoreStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Stok seviyelerini kontrol eder ve gerekirse uyarı gönderir (SignalR bildirim).</summary>
    Task<Result> CheckStockLevelsAndAlertAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Ürün stok geçmişini getirir (tarih filtresi ile, son 100 kayıt).</summary>
    Task<Result<List<StockHistoryResponse>>> GetStockHistoryAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>Merchant stok uyarılarını getirir (aktif olanlar, ürün bilgileri dahil).</summary>
    Task<Result<List<StockAlertResponse>>> GetStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Stok seviyesini manuel günceller (ownership kontrolü, stok geçmişi).</summary>
    Task<Result> UpdateStockLevelAsync(UpdateStockRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu stok güncelleme yapar (transaction, rollback desteği).</summary>
    Task<Result> BulkUpdateStockLevelsAsync(List<UpdateStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Stok raporu oluşturur (özet istatistikler, ürün bazlı detaylar).</summary>
    Task<Result<StockReportResponse>> GetStockReportAsync(StockReportRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Harici sistemlerle stok senkronizasyonu yapar.</summary>
    Task<Result> SynchronizeStockAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
