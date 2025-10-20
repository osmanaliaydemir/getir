using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Enums;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Mağazaları yönetmek için mağaza controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Merchants")]
public class MerchantController : BaseController
{
    private readonly IMerchantService _merchantService;

    public MerchantController(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }

    /// <summary>
    /// Sayfalama ile tüm mağazaları getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış mağaza listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchants(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ID'ye göre mağaza getir
    /// </summary>
    /// <param name="id">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Mağaza detayları</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Benim mağazamı getir (mevcut kullanıcının mağazası)
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Mağaza detayları</returns>
    [HttpGet("my-merchant")]
    [Authorize]
    [Authorize(Roles = "MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyMerchant(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.GetMerchantByOwnerIdAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni mağaza oluştur
    /// </summary>
    /// <param name="request">Mağaza oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan mağaza</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMerchant(
        [FromBody] CreateMerchantRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.CreateMerchantAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağazayı güncelle
    /// </summary>
    /// <param name="id">Mağaza ID'si</param>
    /// <param name="request">Mağaza güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen mağaza</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMerchant(
        [FromRoute] Guid id,
        [FromBody] UpdateMerchantRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.UpdateMerchantAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağazayı sil
    /// </summary>
    /// <param name="id">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMerchant(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _merchantService.DeleteMerchantAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kategori türüne göre mağazaları getir
    /// </summary>
    /// <param name="categoryType">Hizmet kategori türü</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış mağaza listesi</returns>
    [HttpGet("by-category-type/{categoryType}")]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantsByCategoryType(
        [FromRoute] ServiceCategoryType categoryType,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantsByCategoryTypeAsync(categoryType, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kategori türüne göre aktif mağazaları getir (sayfalama yok)
    /// </summary>
    /// <param name="categoryType">Hizmet kategori türü</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Aktif mağaza listesi</returns>
    [HttpGet("active/by-category-type/{categoryType}")]
    [ProducesResponseType(typeof(IEnumerable<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveMerchantsByCategoryType(
        [FromRoute] ServiceCategoryType categoryType,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetActiveMerchantsByCategoryTypeAsync(categoryType, ct);
        return ToActionResult(result);
    }
}
