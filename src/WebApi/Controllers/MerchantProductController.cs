using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant product controller for merchant product management
/// </summary>
[ApiController]
[Route("api/v1/merchants/[controller]")]
[Tags("Merchant Products")]
[Authorize]
public class MerchantProductController : BaseController
{
    private readonly IProductService _productService;

    public MerchantProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get my products
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantProducts(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.GetMyProductsAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="request">Create product request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMerchantProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.CreateMyProductAsync(request, userId, ct);
        if (result.Success)
        {
            return Created($"/api/v1/merchants/products/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update my product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Update product request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMerchantProduct(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.UpdateMyProductAsync(productId, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete my product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMerchantProduct(
        [FromRoute] Guid productId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.DeleteMyProductAsync(productId, userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Get product statistics
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ProductStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStatistics(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.GetMyProductStatisticsAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bulk update product status
    /// </summary>
    /// <param name="request">Bulk update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Bulk update results</returns>
    [HttpPost("bulk-update-status")]
    [ProducesResponseType(typeof(BulkUpdateProductStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateProductStatus(
        [FromBody] BulkUpdateProductStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productService.BulkUpdateMyProductStatusAsync(request, userId, ct);
        return ToActionResult(result);
    }
}
