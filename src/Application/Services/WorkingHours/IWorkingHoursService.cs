using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.WorkingHours;

public interface IWorkingHoursService
{
    Task<Result<List<WorkingHoursResponse>>> GetWorkingHoursByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<Result<WorkingHoursResponse>> GetWorkingHoursByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<WorkingHoursResponse>> CreateWorkingHoursAsync(CreateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<WorkingHoursResponse>> UpdateWorkingHoursAsync(Guid id, UpdateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkingHoursAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> BulkUpdateWorkingHoursAsync(Guid merchantId, BulkUpdateWorkingHoursRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsMerchantOpenAsync(Guid merchantId, DateTime? checkTime = null, CancellationToken cancellationToken = default);
}
