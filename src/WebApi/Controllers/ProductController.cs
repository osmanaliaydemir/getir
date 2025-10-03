using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Product controller for managing products
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Products")]
public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get products by merchant ID
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of products</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByMerchant(
        [FromRoute] Guid merchantId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _productService.GetProductsByMerchantAsync(merchantId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _productService.GetProductByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Create product request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productService.CreateProductAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Update product request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _productService.UpdateProductAsync(id, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _productService.DeleteProductAsync(id, ct);
        return ToActionResult(result);
    }
}
