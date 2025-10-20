using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.ProductOptions;
using Getir.Application.DTO;
using Getir.Application.Common;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Pazar ürün varyant yönetimi controller'ı
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
    /// Bir pazar ürünü için ürün varyantlarını getirir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <returns>Ürün varyantları listesi</returns>
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
    /// Belirli bir ürün varyantını getirir
    /// </summary>
    /// <param name="id">Varyant ID</param>
    /// <returns>Ürün varyant detayları</returns>
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
    /// Yeni ürün varyantı oluşturur
    /// </summary>
    /// <param name="request">Varyant oluşturma isteği</param>
    /// <returns>Oluşturulan varyant</returns>
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
    /// Ürün varyantını günceller
    /// </summary>
    /// <param name="id">Varyant ID</param>
    /// <param name="request">Güncelleme isteği</param>
    /// <returns>Güncellenen varyant</returns>
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
    /// Ürün varyantını siler
    /// </summary>
    /// <param name="id">Varyant ID</param>
    /// <returns>İçerik yok</returns>
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
    /// Varyant stok miktarını günceller
    /// </summary>
    /// <param name="id">Varyant ID</param>
    /// <param name="newStockQuantity">Yeni stok miktarı</param>
    /// <returns>İçerik yok</returns>
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
    /// Varyant stok miktarlarını toplu günceller
    /// </summary>
    /// <param name="requests">Stok güncelleme istekleri listesi</param>
    /// <returns>İçerik yok</returns>
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
