using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.ProductOptions;
using Getir.Application.DTO;
using Getir.Application.Common;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Product option management controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Product Options")]
public class ProductOptionController : BaseController
{
    private readonly IProductOptionService _productOptionService;
    private readonly IProductOptionGroupService _productOptionGroupService;

    public ProductOptionController(
        IProductOptionService productOptionService,
        IProductOptionGroupService productOptionGroupService)
    {
        _productOptionService = productOptionService;
        _productOptionGroupService = productOptionGroupService;
    }

    /// <summary>
    /// Get product option groups for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="query">Pagination query</param>
    /// <returns>List of product option groups</returns>
    [HttpGet("groups/{productId}")]
    [ProducesResponseType(typeof(PagedResult<ProductOptionGroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOptionGroups(
        Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _productOptionGroupService.GetProductOptionGroupsAsync(
            productId, query, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Get a specific product option group
    /// </summary>
    /// <param name="id">Option group ID</param>
    /// <returns>Product option group details</returns>
    [HttpGet("groups/details/{id}")]
    [ProducesResponseType(typeof(ProductOptionGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOptionGroup(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _productOptionGroupService.GetProductOptionGroupAsync(id, cancellationToken);
        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Create a new product option group
    /// </summary>
    /// <param name="request">Create option group request</param>
    /// <returns>Created option group</returns>
    [HttpPost("groups")]
    [ProducesResponseType(typeof(ProductOptionGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductOptionGroup(
        [FromBody] CreateProductOptionGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionGroupService.CreateProductOptionGroupAsync(
            request, merchantOwnerId, cancellationToken);

        return result.Success ? CreatedAtAction(nameof(GetProductOptionGroup), 
            new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Update a product option group
    /// </summary>
    /// <param name="id">Option group ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated option group</returns>
    [HttpPut("groups/{id}")]
    [ProducesResponseType(typeof(ProductOptionGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductOptionGroup(
        Guid id,
        [FromBody] UpdateProductOptionGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionGroupService.UpdateProductOptionGroupAsync(
            id, request, merchantOwnerId, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Delete a product option group
    /// </summary>
    /// <param name="id">Option group ID</param>
    /// <returns>No content</returns>
    [HttpDelete("groups/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductOptionGroup(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionGroupService.DeleteProductOptionGroupAsync(
            id, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Reorder product option groups
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="orderedGroupIds">Ordered list of group IDs</param>
    /// <returns>No content</returns>
    [HttpPut("groups/{productId}/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderProductOptionGroups(
        Guid productId,
        [FromBody] List<Guid> orderedGroupIds,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionGroupService.ReorderProductOptionGroupsAsync(
            productId, orderedGroupIds, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Get product options for a group
    /// </summary>
    /// <param name="productOptionGroupId">Option group ID</param>
    /// <param name="query">Pagination query</param>
    /// <returns>List of product options</returns>
    [HttpGet("groups/{productOptionGroupId}/options")]
    [ProducesResponseType(typeof(PagedResult<ProductOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductOptions(
        Guid productOptionGroupId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _productOptionService.GetProductOptionsAsync(
            productOptionGroupId, query, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Get a specific product option
    /// </summary>
    /// <param name="id">Option ID</param>
    /// <returns>Product option details</returns>
    [HttpGet("options/{id}")]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOption(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _productOptionService.GetProductOptionAsync(id, cancellationToken);
        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Create a new product option
    /// </summary>
    /// <param name="request">Create option request</param>
    /// <returns>Created option</returns>
    [HttpPost("options")]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductOption(
        [FromBody] CreateProductOptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionService.CreateProductOptionAsync(
            request, merchantOwnerId, cancellationToken);

        return result.Success ? CreatedAtAction(nameof(GetProductOption), 
            new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Update a product option
    /// </summary>
    /// <param name="id">Option ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated option</returns>
    [HttpPut("options/{id}")]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductOption(
        Guid id,
        [FromBody] UpdateProductOptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionService.UpdateProductOptionAsync(
            id, request, merchantOwnerId, cancellationToken);

        return result.Success ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Delete a product option
    /// </summary>
    /// <param name="id">Option ID</param>
    /// <returns>No content</returns>
    [HttpDelete("options/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductOption(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionService.DeleteProductOptionAsync(
            id, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Bulk create product options
    /// </summary>
    /// <param name="request">Bulk create request</param>
    /// <returns>No content</returns>
    [HttpPost("options/bulk")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreateProductOptions(
        [FromBody] BulkCreateProductOptionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionService.BulkCreateProductOptionsAsync(
            request, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Bulk update product options
    /// </summary>
    /// <param name="request">Bulk update request</param>
    /// <returns>No content</returns>
    [HttpPut("options/bulk")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdateProductOptions(
        [FromBody] BulkUpdateProductOptionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var merchantOwnerId);
        if (unauthorizedResult != null) return unauthorizedResult;
        var result = await _productOptionService.BulkUpdateProductOptionsAsync(
            request, merchantOwnerId, cancellationToken);

        return result.Success ? NoContent() : ToActionResult(result);
    }
}