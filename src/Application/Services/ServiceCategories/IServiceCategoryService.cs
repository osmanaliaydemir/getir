using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.ServiceCategories;

/// <summary>
/// Servis kategorisi servisi: merchant hizmet kategorileri (Food/Market/Pharmacy/Service), tip bazlı filtreleme.
/// </summary>
public interface IServiceCategoryService
{
    /// <summary>Servis kategorilerini sayfalama ile getirir (cache).</summary>
    Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Servis kategorisini ID ile getirir (cache).</summary>
    Task<Result<ServiceCategoryResponse>> GetServiceCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni servis kategorisi oluşturur (cache invalidation).</summary>
    Task<Result<ServiceCategoryResponse>> CreateServiceCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    /// <summary>Servis kategorisini günceller (cache invalidation).</summary>
    Task<Result<ServiceCategoryResponse>> UpdateServiceCategoryAsync(Guid id, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    /// <summary>Servis kategorisini siler (soft delete, cache invalidation).</summary>
    Task<Result> DeleteServiceCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Tip bazlı servis kategorilerini getirir (cache).</summary>
    Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesByTypeAsync(ServiceCategoryType type, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Tip bazlı aktif kategorileri getirir (tüm kayıtlar, cache).</summary>
    Task<Result<IEnumerable<ServiceCategoryResponse>>> GetActiveServiceCategoriesByTypeAsync(ServiceCategoryType type, CancellationToken cancellationToken = default);
}

