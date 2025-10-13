using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IWorkingHoursService
{
    Task<List<WorkingHoursResponse>?> GetWorkingHoursByMerchantAsync(Guid merchantId, CancellationToken ct = default);
    Task<bool> BulkUpdateWorkingHoursAsync(Guid merchantId, List<UpdateWorkingHoursRequest> workingHours, CancellationToken ct = default);
}

