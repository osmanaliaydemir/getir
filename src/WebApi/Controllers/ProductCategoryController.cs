using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Product category controller for managing product categories
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Product Categories")]
public class ProductCategoryController : BaseController
{
    private readonly IProductCategoryService _productCategoryService;

    public ProductCategoryController(IProductCategoryService productCategoryService)
    {
        _productCategoryService = productCategoryService;
    }

    /// <summary>
    /// Get merchant's categories (flat list)
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of categories</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantCategories(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetMerchantCategoriesAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchant's category tree
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Category tree</returns>
    [HttpGet("merchant/{merchantId:guid}/tree")]
    [ProducesResponseType(typeof(List<ProductCategoryTreeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantCategoryTree(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetMerchantCategoryTreeAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get product category by ID
    /// </summary>
    /// <param name="id">Product category ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product category details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductCategoryById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetProductCategoryByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new product category for merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="request">Create product category request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product category</returns>
    [HttpPost("merchant/{merchantId:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProductCategory(
        [FromRoute] Guid merchantId,
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productCategoryService.CreateProductCategoryAsync(request, merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update product category
    /// </summary>
    /// <param name="id">Product category ID</param>
    /// <param name="request">Update product category request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated product category</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productCategoryService.UpdateProductCategoryAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete product category
    /// </summary>
    /// <param name="id">Product category ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductCategory(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productCategoryService.DeleteProductCategoryAsync(id, userId, ct);
        return ToActionResult(result);
    }
}
