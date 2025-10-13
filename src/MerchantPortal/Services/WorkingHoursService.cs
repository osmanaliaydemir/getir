using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<WorkingHoursService> _logger;

    public WorkingHoursService(IApiClient apiClient, ILogger<WorkingHoursService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<WorkingHoursResponse>?> GetWorkingHoursByMerchantAsync(Guid merchantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<WorkingHoursResponse>>>(
                $"api/v1/workinghours/merchant/{merchantId}",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting working hours for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    public async Task<bool> BulkUpdateWorkingHoursAsync(Guid merchantId, List<UpdateWorkingHoursRequest> workingHours, CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                MerchantId = merchantId,
                WorkingHours = workingHours
            };

            var response = await _apiClient.PutAsync<ApiResponse<object>>(
                $"api/v1/workinghours/merchant/{merchantId}/bulk",
                request,
                ct);

            return response?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating working hours for merchant {MerchantId}", merchantId);
            return false;
        }
    }
}

