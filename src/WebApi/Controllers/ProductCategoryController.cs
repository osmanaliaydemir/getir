using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Ürün kategorilerini yönetmek için ürün kategorisi controller'ı
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
    /// Mağaza kategorilerini getir (düz liste)
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kategori listesi</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantCategories(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetMerchantCategoriesAsync(merchantId, ct);
        return ToActionResult<List<ProductCategoryResponse>>(result);
    }

    /// <summary>
    /// Mağaza kategori ağacını getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kategori ağacı</returns>
    [HttpGet("merchant/{merchantId:guid}/tree")]
    [ProducesResponseType(typeof(List<ProductCategoryTreeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantCategoryTree(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetMerchantCategoryTreeAsync(merchantId, ct);
        return ToActionResult<List<ProductCategoryTreeResponse>>(result);
    }

    /// <summary>
    /// Ürün kategorisini ID'ye göre getir
    /// </summary>
    /// <param name="id">Ürün kategorisi ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ürün kategorisi detayları</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductCategoryById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _productCategoryService.GetProductCategoryByIdAsync(id, ct);
        return ToActionResult<ProductCategoryResponse>(result);
    }

    /// <summary>
    /// Mağaza için yeni ürün kategorisi oluştur
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="request">Ürün kategorisi oluşturma isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan ürün kategorisi</returns>
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
    /// Ürün kategorisini güncelle
    /// </summary>
    /// <param name="id">Ürün kategorisi ID</param>
    /// <param name="request">Ürün kategorisi güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen ürün kategorisi</returns>
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
    /// Ürün kategorisini sil
    /// </summary>
    /// <param name="id">Ürün kategorisi ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
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
