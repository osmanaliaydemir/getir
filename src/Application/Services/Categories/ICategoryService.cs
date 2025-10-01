using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Categories;

public interface ICategoryService
{
    Task<Result<PagedResult<CategoryResponse>>> GetCategoriesAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default);
    
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<CategoryResponse>> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
