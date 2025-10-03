using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ServiceCategories;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Service category controller for managing service categories
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
    /// Get service categories with pagination
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of service categories</returns>
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
    /// Get service category by ID
    /// </summary>
    /// <param name="id">Service category ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Service category details</returns>
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
    /// Create a new service category
    /// </summary>
    /// <param name="request">Create service category request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created service category</returns>
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
    /// Update service category
    /// </summary>
    /// <param name="id">Service category ID</param>
    /// <param name="request">Update service category request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated service category</returns>
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
    /// Delete service category
    /// </summary>
    /// <param name="id">Service category ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Get service categories by type
    /// </summary>
    /// <param name="type">Service category type</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of service categories</returns>
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
    /// Get active service categories by type (without pagination)
    /// </summary>
    /// <param name="type">Service category type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active service categories</returns>
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
