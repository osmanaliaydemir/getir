using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Mağaza ürün yönetimi için mağaza ürün controller'ı
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
    /// Mağazama ait ürünleri getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış ürünler</returns>
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
    /// Yeni ürün oluştur
    /// </summary>
    /// <param name="request">Ürün oluşturma isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan ürün</returns>
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
            return Created($"/api/v1/merchants/products/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Ürünümü güncelle
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="request">Ürün güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen ürün</returns>
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
    /// Ürünümü sil
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>İçerik yok</returns>
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
    /// Ürün istatistiklerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ürün istatistikleri</returns>
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
    /// Ürün durumlarını toplu güncelle
    /// </summary>
    /// <param name="request">Toplu güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Toplu güncelleme sonuçları</returns>
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
