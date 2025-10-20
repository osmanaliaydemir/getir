using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.WorkingHours;

/// <summary>
/// Çalışma saatleri servisi: merchant günlük çalışma saatleri, toplu güncelleme, açık/kapalı kontrol, cache.
/// </summary>
public interface IWorkingHoursService
{
    /// <summary>Merchant çalışma saatlerini getirir (7 gün, cache).</summary>
    Task<Result<List<WorkingHoursResponse>>> GetWorkingHoursByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Çalışma saatini ID ile getirir.</summary>
    Task<Result<WorkingHoursResponse>> GetWorkingHoursByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni çalışma saati oluşturur (ownership kontrolü, duplicate kontrolü, cache invalidation).</summary>
    Task<Result<WorkingHoursResponse>> CreateWorkingHoursAsync(CreateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Çalışma saatini günceller (ownership kontrolü, cache invalidation).</summary>
    Task<Result<WorkingHoursResponse>> UpdateWorkingHoursAsync(Guid id, UpdateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Çalışma saatini siler (ownership kontrolü, cache invalidation).</summary>
    Task<Result> DeleteWorkingHoursAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu çalışma saati güncelleme (7 gün, upsert, cache invalidation).</summary>
    Task<Result> BulkUpdateWorkingHoursAsync(Guid merchantId, BulkUpdateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Merchant'ın belirtilen zamanda açık olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> IsMerchantOpenAsync(Guid merchantId, DateTime? checkTime = null, CancellationToken cancellationToken = default);
}
