using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

/// <summary>
/// Sistem değişiklik log interface'i: değişiklik oluşturma, sorgulama, istatistik ve temizlik işlemleri.
/// </summary>
public interface ISystemChangeLogService
{
    /// <summary>Sistem değişiklik kaydı oluşturur.</summary>
    Task<Result<SystemChangeLogResponse>> CreateSystemChangeLogAsync(CreateSystemChangeLogRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>ID'ye göre sistem değişiklik kaydını getirir.</summary>
    Task<Result<SystemChangeLogResponse>> GetSystemChangeLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Filtre ile sistem değişiklik kayıtlarını getirir.</summary>
    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsAsync(SystemChangeQueryRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Varlığa ait sistem değişiklik kayıtlarını getirir.</summary>
    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByEntityAsync(string entityType, string entityId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Kullanıcıya ait sistem değişiklik kayıtlarını getirir.</summary>
    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Sistem değişiklik istatistiklerini getirir.</summary>
    Task<Result<Dictionary<string, int>>> GetSystemChangeStatisticsAsync(DateTime startDate, DateTime endDate, string? entityType = null, CancellationToken cancellationToken = default);
    
    /// <summary>Son sistem değişikliklerini getirir.</summary>
    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetRecentSystemChangesAsync(int count = 50, string? entityType = null, CancellationToken cancellationToken = default);
    
    /// <summary>Eski sistem değişiklik kayıtlarını siler.</summary>
    Task<Result> DeleteOldSystemChangeLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
