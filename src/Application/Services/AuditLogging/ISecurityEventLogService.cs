using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

/// <summary>
/// Güvenlik olay log interface'i: olay oluşturma, sorgulama, çözümleme ve istatistik işlemleri.
/// </summary>
public interface ISecurityEventLogService
{
    /// <summary>Güvenlik olay kaydı oluşturur.</summary>
    Task<Result<SecurityEventLogResponse>> CreateSecurityEventLogAsync(CreateSecurityEventLogRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>ID'ye göre güvenlik olayını getirir.</summary>
    Task<Result<SecurityEventLogResponse>> GetSecurityEventLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Filtre ile güvenlik olaylarını getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventLogsAsync(SecurityEventQueryRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Çözülmemiş güvenlik olaylarını getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetUnresolvedSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null, string? severity = null, string? riskLevel = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>İnceleme gerektiren olayları getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsRequiringInvestigationAsync(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Güvenlik olayını çözer.</summary>
    Task<Result<SecurityEventLogResponse>> ResolveSecurityEventAsync(Guid id, string resolvedBy, string resolutionNotes, CancellationToken cancellationToken = default);
    
    /// <summary>Yanlış pozitif olarak işaretler.</summary>
    Task<Result<SecurityEventLogResponse>> MarkAsFalsePositiveAsync(Guid id, string resolvedBy, string resolutionNotes, CancellationToken cancellationToken = default);
    
    /// <summary>Güvenlik olay istatistiklerini getirir.</summary>
    Task<Result<Dictionary<string, int>>> GetSecurityEventStatisticsAsync(DateTime startDate, DateTime endDate, string? severity = null, string? riskLevel = null, CancellationToken cancellationToken = default);
    
    /// <summary>Yüksek riskli olayları getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetHighRiskSecurityEventsAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Kullanıcıya ait güvenlik olaylarını getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>IP adresine ait güvenlik olaylarını getirir.</summary>
    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByIpAddressAsync(string ipAddress, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Eski güvenlik olay loglarını siler.</summary>
    Task<Result> DeleteOldSecurityEventLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
