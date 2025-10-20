using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stok senkronizasyon servisi: harici sistem entegrasyonu, geçmiş, bağlantı testi, otomatik sync zamanlama.
/// </summary>
public interface IStockSyncService
{
    /// <summary>Harici sistem ile stok senkronizasyonu yapar (toplu güncelleme, hata yönetimi).</summary>
    Task<Result<StockSyncResponse>> SynchronizeWithExternalSystemAsync(StockSyncRequest request, CancellationToken cancellationToken = default);
    /// <summary>Senkronizasyon geçmişini getirir (tarih filtresi ile).</summary>
    Task<Result<List<StockSyncHistoryResponse>>> GetSynchronizationHistoryAsync(Guid merchantId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>Senkronizasyon durumunu getirir (etkin/son sync zamanı).</summary>
    Task<Result<StockSyncStatusResponse>> GetSynchronizationStatusAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Harici sistem bağlantısını yapılandırır (API key/URL, sync aralığı).</summary>
    Task<Result> ConfigureExternalSystemAsync(ExternalSystemConfigRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Harici sistem bağlantısını test eder.</summary>
    Task<Result<ConnectionTestResponse>> TestExternalSystemConnectionAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Otomatik senkronizasyon zamanlar (dakika aralığı ile).</summary>
    Task<Result> ScheduleAutomaticSyncAsync(Guid merchantId, int intervalMinutes, CancellationToken cancellationToken = default);
    /// <summary>Otomatik senkronizasyonu iptal eder.</summary>
    Task<Result> CancelAutomaticSyncAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
