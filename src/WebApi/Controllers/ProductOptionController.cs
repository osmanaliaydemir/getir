using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Product option controller for product option management
/// </summary>
[ApiController]
[Route("api/v1/merchants/products/{productId:guid}/[controller]")]
[Tags("Product Options")]
[Authorize]
public class ProductOptionController : BaseController
{
    private readonly IProductOptionService _productOptionService;

    public ProductOptionController(IProductOptionService productOptionService)
    {
        _productOptionService = productOptionService;
    }

    /// <summary>
    /// Get product options
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged product options</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOptions(
        [FromRoute] Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _productOptionService.GetProductOptionsAsync(productId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get product option by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="optionId">Option ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product option</returns>
    [HttpGet("{optionId:guid}")]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOption(
        [FromRoute] Guid productId,
        [FromRoute] Guid optionId,
        CancellationToken ct = default)
    {
        var result = await _productOptionService.GetProductOptionAsync(optionId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create product option
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Create option request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created option</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProductOption(
        [FromRoute] Guid productId,
        [FromBody] CreateProductOptionRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productOptionService.CreateProductOptionAsync(request, productId, ct);
        if (result.Success)
        {
            return Created($"/api/v1/merchants/products/{productId}/options/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update product option
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="optionId">Option ID</param>
    /// <param name="request">Update option request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated option</returns>
    [HttpPut("{optionId:guid}")]
    [ProducesResponseType(typeof(ProductOptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductOption(
        [FromRoute] Guid productId,
        [FromRoute] Guid optionId,
        [FromBody] UpdateProductOptionRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productOptionService.UpdateProductOptionAsync(optionId, request, productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete product option
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="optionId">Option ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{optionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductOption(
        [FromRoute] Guid productId,
        [FromRoute] Guid optionId,
        CancellationToken ct = default)
    {
        var result = await _productOptionService.DeleteProductOptionAsync(productId, optionId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }
}

/// <summary>
/// Product option group controller for product option group management
/// </summary>
[ApiController]
[Route("api/v1/merchants/products/{productId:guid}/option-groups")]
[Tags("Product Option Groups")]
[Authorize]
public class ProductOptionGroupController : BaseController
{
    private readonly IProductOptionGroupService _productOptionGroupService;

    public ProductOptionGroupController(IProductOptionGroupService productOptionGroupService)
    {
        _productOptionGroupService = productOptionGroupService;
    }

    /// <summary>
    /// Get product option groups
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged option groups</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductOptionGroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOptionGroups(
        [FromRoute] Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _productOptionGroupService.GetProductOptionGroupsAsync(productId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get product option groups with options (for product display)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Option groups with options</returns>
    [HttpGet("with-options")]
    [ProducesResponseType(typeof(List<ProductOptionGroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductOptionGroupsWithOptions(
        [FromRoute] Guid productId,
        CancellationToken ct = default)
    {
        var result = await _productOptionGroupService.GetProductOptionGroupsWithOptionsAsync(productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create product option group
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Create option group request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created option group</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductOptionGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProductOptionGroup(
        [FromRoute] Guid productId,
        [FromBody] CreateProductOptionGroupRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productOptionGroupService.CreateProductOptionGroupAsync(request, productId, ct);
        if (result.Success)
        {
            return Created($"/api/v1/merchants/products/{productId}/option-groups/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update product option group
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="groupId">Group ID</param>
    /// <param name="request">Update option group request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated option group</returns>
    [HttpPut("{groupId:guid}")]
    [ProducesResponseType(typeof(ProductOptionGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductOptionGroup(
        [FromRoute] Guid productId,
        [FromRoute] Guid groupId,
        [FromBody] UpdateProductOptionGroupRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productOptionGroupService.UpdateProductOptionGroupAsync(groupId, request, productId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete product option group
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="groupId">Group ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductOptionGroup(
        [FromRoute] Guid productId,
        [FromRoute] Guid groupId,
        CancellationToken ct = default)
    {
        var result = await _productOptionGroupService.DeleteProductOptionGroupAsync(productId, groupId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }
}
