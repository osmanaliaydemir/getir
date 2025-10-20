using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ServiceCategories;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Hizmet kategorilerini yönetmek için hizmet kategorisi controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Service Categories")]
public class ServiceCategoryController : BaseController
{
    private readonly IServiceCategoryService _serviceCategoryService;

    public ServiceCategoryController(IServiceCategoryService serviceCategoryService)
    {
        _serviceCategoryService = serviceCategoryService;
    }

    /// <summary>
    /// Sayfalama ile hizmet kategorilerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış hizmet kategorileri</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ServiceCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceCategories(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _serviceCategoryService.GetServiceCategoriesAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Hizmet kategorisini ID'ye göre getir
    /// </summary>
    /// <param name="id">Hizmet kategorisi ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Hizmet kategorisi detayları</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceCategoryById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _serviceCategoryService.GetServiceCategoryByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni hizmet kategorisi oluştur
    /// </summary>
    /// <param name="request">Hizmet kategorisi oluşturma isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan hizmet kategorisi</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateServiceCategory(
        [FromBody] CreateServiceCategoryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _serviceCategoryService.CreateServiceCategoryAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Hizmet kategorisini güncelle
    /// </summary>
    /// <param name="id">Hizmet kategorisi ID</param>
    /// <param name="request">Hizmet kategorisi güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen hizmet kategorisi</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _serviceCategoryService.UpdateServiceCategoryAsync(id, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Hizmet kategorisini sil
    /// </summary>
    /// <param name="id">Hizmet kategorisi ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceCategory(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _serviceCategoryService.DeleteServiceCategoryAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Türüne göre hizmet kategorilerini getir
    /// </summary>
    /// <param name="type">Hizmet kategorisi türü</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış hizmet kategorileri</returns>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(PagedResult<ServiceCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceCategoriesByType(
        [FromRoute] ServiceCategoryType type,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _serviceCategoryService.GetServiceCategoriesByTypeAsync(type, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Türüne göre aktif hizmet kategorilerini getir (sayfalama yok)
    /// </summary>
    /// <param name="type">Hizmet kategorisi türü</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Aktif hizmet kategorileri listesi</returns>
    [HttpGet("active/by-type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<ServiceCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveServiceCategoriesByType(
        [FromRoute] ServiceCategoryType type,
        CancellationToken ct = default)
    {
        var result = await _serviceCategoryService.GetActiveServiceCategoriesByTypeAsync(type, ct);
        return ToActionResult(result);
    }
}
