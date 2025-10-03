using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.ServiceCategories;

public interface IServiceCategoryService
{
    Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default);
    
    Task<Result<ServiceCategoryResponse>> GetServiceCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<ServiceCategoryResponse>> CreateServiceCategoryAsync(
        CreateServiceCategoryRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<ServiceCategoryResponse>> UpdateServiceCategoryAsync(
        Guid id,
        UpdateServiceCategoryRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteServiceCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesByTypeAsync(
        ServiceCategoryType type,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<ServiceCategoryResponse>>> GetActiveServiceCategoriesByTypeAsync(
        ServiceCategoryType type,
        CancellationToken cancellationToken = default);
}

