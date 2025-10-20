using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.ProductOptions;
using Getir.Application.DTO;
using Getir.Application.Common;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Ürün seçenek yönetimi controller'ı
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
    /// Bir ürün için ürün seçenek gruplarını getirir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <returns>Ürün seçenek grupları listesi</returns>
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
    /// Belirli bir ürün seçenek grubunu getirir
    /// </summary>
    /// <param name="id">Seçenek grubu ID</param>
    /// <returns>Ürün seçenek grubu detayları</returns>
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
    /// Yeni ürün seçenek grubu oluşturur
    /// </summary>
    /// <param name="request">Seçenek grubu oluşturma isteği</param>
    /// <returns>Oluşturulan seçenek grubu</returns>
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
    /// Ürün seçenek grubunu günceller
    /// </summary>
    /// <param name="id">Seçenek grubu ID</param>
    /// <param name="request">Güncelleme isteği</param>
    /// <returns>Güncellenen seçenek grubu</returns>
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
    /// Ürün seçenek grubunu siler
    /// </summary>
    /// <param name="id">Seçenek grubu ID</param>
    /// <returns>İçerik yok</returns>
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
    /// Ürün seçenek gruplarını yeniden sıralar
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="orderedGroupIds">Sıralı grup ID listesi</param>
    /// <returns>İçerik yok</returns>
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
    /// Bir grup için ürün seçeneklerini getirir
    /// </summary>
    /// <param name="productOptionGroupId">Seçenek grubu ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <returns>Ürün seçenekleri listesi</returns>
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
    /// Belirli bir ürün seçeneğini getirir
    /// </summary>
    /// <param name="id">Seçenek ID</param>
    /// <returns>Ürün seçeneği detayları</returns>
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
    /// Yeni ürün seçeneği oluşturur
    /// </summary>
    /// <param name="request">Seçenek oluşturma isteği</param>
    /// <returns>Oluşturulan seçenek</returns>
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
    /// Ürün seçeneğini günceller
    /// </summary>
    /// <param name="id">Seçenek ID</param>
    /// <param name="request">Güncelleme isteği</param>
    /// <returns>Güncellenen seçenek</returns>
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
    /// Ürün seçeneğini siler
    /// </summary>
    /// <param name="id">Seçenek ID</param>
    /// <returns>İçerik yok</returns>
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
    /// Ürün seçeneklerini toplu oluşturur
    /// </summary>
    /// <param name="request">Toplu oluşturma isteği</param>
    /// <returns>İçerik yok</returns>
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
    /// Ürün seçeneklerini toplu günceller
    /// </summary>
    /// <param name="request">Toplu güncelleme isteği</param>
    /// <returns>İçerik yok</returns>
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