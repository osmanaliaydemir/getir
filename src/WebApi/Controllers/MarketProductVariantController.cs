using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.ProductOptions;
using Getir.Application.DTO;
using Getir.Application.Common;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Market product variant management controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Market Product Variants")]
public class MarketProductVariantController : BaseController
{
    private readonly IMarketProductVariantService _variantService;

    public MarketProductVariantController(IMarketProductVariantService variantService)
    {
        _variantService = variantService;
    }

    /// <summary>
    /// Get product variants for a market product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="query">Pagination query</param>
    /// <returns>List of product variants</returns>
    [HttpGet("products/{productId}")]
    [ProducesResponseType(typeof(PagedResult<MarketProductVariantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductVariants(
        Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _variantService.GetProductVariantsAsync(
            productId, query, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Get a specific product variant
    /// </summary>
    /// <param name="id">Variant ID</param>
    /// <returns>Product variant details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MarketProductVariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductVariant(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _variantService.GetProductVariantAsync(id, cancellationToken);
        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Create a new product variant
    /// </summary>
    /// <param name="request">Create variant request</param>
    /// <returns>Created variant</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MarketProductVariantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductVariant(
        [FromBody] CreateMarketProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _variantService.CreateProductVariantAsync(
            request, merchantOwnerId, cancellationToken);

        return result.Success ? CreatedAtAction(nameof(GetProductVariant), 
            new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Update a product variant
    /// </summary>
    /// <param name="id">Variant ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated variant</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MarketProductVariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductVariant(
        Guid id,
        [FromBody] UpdateMarketProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _variantService.UpdateProductVariantAsync(
            id, request, merchantOwnerId, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Delete a product variant
    /// </summary>
    /// <param name="id">Variant ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductVariant(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _variantService.DeleteProductVariantAsync(
            id, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Update variant stock quantity
    /// </summary>
    /// <param name="id">Variant ID</param>
    /// <param name="newStockQuantity">New stock quantity</param>
    /// <returns>No content</returns>
    [HttpPut("{id}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariantStock(
        Guid id,
        [FromBody] int newStockQuantity,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _variantService.UpdateVariantStockAsync(
            id, newStockQuantity, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Bulk update variant stock quantities
    /// </summary>
    /// <param name="requests">List of stock update requests</param>
    /// <returns>No content</returns>
    [HttpPut("stock/bulk")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdateVariantStock(
        [FromBody] List<UpdateVariantStockRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _variantService.BulkUpdateVariantStockAsync(
            requests, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }
}
