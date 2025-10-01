using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Merchants;

public interface IMerchantService
{
    Task<Result<PagedResult<MerchantResponse>>> GetMerchantsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> GetMerchantByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> CreateMerchantAsync(
        CreateMerchantRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> UpdateMerchantAsync(
        Guid id,
        UpdateMerchantRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteMerchantAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
