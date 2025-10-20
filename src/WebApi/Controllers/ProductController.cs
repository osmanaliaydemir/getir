using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Ürünleri yönetmek için ürün controller'ı
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
    /// Mağaza ID'sine göre ürünleri getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış ürün listesi</returns>
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
    /// Popüler ürünleri getir (en çok satılan ve yüksek ratingli)
    /// Ana sayfa için cache'li endpoint
    /// </summary>
    /// <param name="limit">Döndürülecek maksimum ürün sayısı (varsayılan: 10)</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Popüler ürün listesi</returns>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularProducts(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await _productService.GetPopularProductsAsync(limit, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ID'ye göre ürün getir
    /// </summary>
    /// <param name="id">Ürün ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ürün detayları</returns>
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
    /// Yeni ürün oluştur
    /// </summary>
    /// <param name="request">Ürün oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan ürün</returns>
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
    /// Ürünü güncelle
    /// </summary>
    /// <param name="id">Ürün ID'si</param>
    /// <param name="request">Ürün güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen ürün</returns>
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
