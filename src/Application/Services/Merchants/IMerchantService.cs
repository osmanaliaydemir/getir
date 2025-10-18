using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Merchants;

public interface IMerchantService
{
    Task<Result<PagedResult<MerchantResponse>>> GetMerchantsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> GetMerchantByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> GetMerchantByOwnerIdAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> CreateMerchantAsync(
        CreateMerchantRequest request,
        Guid ownerId,
        CancellationToken cancellationToken = default);
    
    Task<Result<MerchantResponse>> UpdateMerchantAsync(
        Guid id,
        UpdateMerchantRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteMerchantAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<MerchantResponse>>> GetMerchantsByCategoryTypeAsync(
        ServiceCategoryType categoryType,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<MerchantResponse>>> GetActiveMerchantsByCategoryTypeAsync(
        ServiceCategoryType categoryType,
        CancellationToken cancellationToken = default);
}
