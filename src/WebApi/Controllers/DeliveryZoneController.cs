using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.DeliveryZones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Teslimat bölgelerini yönetmek için teslimat bölgesi controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Delivery Zones")]
public class DeliveryZoneController : BaseController
{
    private readonly IDeliveryZoneService _deliveryZoneService;

    public DeliveryZoneController(IDeliveryZoneService deliveryZoneService)
    {
        _deliveryZoneService = deliveryZoneService;
    }

    /// <summary>
    /// Bir mağaza için teslimat bölgelerini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Teslimat bölgeleri listesi</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<DeliveryZoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryZonesByMerchant(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _deliveryZoneService.GetDeliveryZonesByMerchantAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Belirli bir teslimat bölgesini getir
    /// </summary>
    /// <param name="id">Teslimat bölgesi ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Teslimat bölgesi detayları</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryZoneById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _deliveryZoneService.GetDeliveryZoneByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimat bölgesi oluştur
    /// </summary>
    /// <param name="request">Teslimat bölgesi oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan teslimat bölgesi</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDeliveryZone(
        [FromBody] CreateDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.CreateDeliveryZoneAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimat bölgesini güncelle
    /// </summary>
    /// <param name="id">Teslimat bölgesi ID'si</param>
    /// <param name="request">Teslimat bölgesi güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen teslimat bölgesi</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeliveryZone(
        [FromRoute] Guid id,
        [FromBody] UpdateDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.UpdateDeliveryZoneAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimat bölgesini sil
    /// </summary>
    /// <param name="id">Teslimat bölgesi ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeliveryZone(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.DeleteDeliveryZoneAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Konum teslimat bölgesi içinde mi kontrol et
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="request">Teslimat bölgesi kontrol talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kontrol sonucu</returns>
    [HttpPost("merchant/{merchantId:guid}/check")]
    [ProducesResponseType(typeof(CheckDeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckDeliveryZone(
        [FromRoute] Guid merchantId,
        [FromBody] CheckDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _deliveryZoneService.CheckDeliveryZoneAsync(merchantId, request, ct);
        return ToActionResult(result);
    }
}
