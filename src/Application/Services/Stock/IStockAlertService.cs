using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stok uyarı servisi: low/out/overstock uyarıları, istatistikler, bildirimler, ayar yönetimi.
/// </summary>
public interface IStockAlertService
{
    /// <summary>Düşük stok ürünleri için uyarı oluşturur (duplicate kontrolü, SignalR bildirim).</summary>
    Task<Result> CreateLowStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Stok tükenen ürünler için uyarı oluşturur (duplicate kontrolü, SignalR bildirim).</summary>
    Task<Result> CreateOutOfStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Aşırı stok için uyarı oluşturur (duplicate kontrolü, SignalR bildirim).</summary>
    Task<Result> CreateOverstockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Stok uyarısını çözer (çözüm notları ile).</summary>
    Task<Result> ResolveStockAlertAsync(Guid alertId, Guid resolvedBy, string resolutionNotes, CancellationToken cancellationToken = default);
    /// <summary>Stok uyarı istatistiklerini getirir (tip bazlı sayılar, çözülme durumu).</summary>
    Task<Result<StockAlertStatisticsResponse>> GetStockAlertStatisticsAsync(Guid merchantId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>Stok uyarı bildirimleri gönderir (SignalR ile gerçek zamanlı).</summary>
    Task<Result> SendStockAlertNotificationsAsync(List<Guid> alertIds, CancellationToken cancellationToken = default);
    /// <summary>Stok uyarı ayarlarını yapılandırır (eşik değerleri, otomatik stok düşürme, senkronizasyon).</summary>
    Task<Result> ConfigureStockAlertSettingsAsync(StockAlertSettingsRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
