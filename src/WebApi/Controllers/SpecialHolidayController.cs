using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.SpecialHolidays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Özel tatil günleri ve geçici kapanış yönetimi
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Special Holidays")]
public class SpecialHolidayController : BaseController
{
    private readonly ISpecialHolidayService _specialHolidayService;

    public SpecialHolidayController(ISpecialHolidayService specialHolidayService)
    {
        _specialHolidayService = specialHolidayService;
    }

    /// <summary>
    /// Tüm aktif özel tatilleri listele
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Özel tatil listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SpecialHolidayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSpecialHolidays(CancellationToken ct = default)
    {
        var result = await _specialHolidayService.GetAllSpecialHolidaysAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant'a ait özel tatilleri getir
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="includeInactive">Pasif olanları da dahil et</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Özel tatil listesi</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<SpecialHolidayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecialHolidaysByMerchant(
        [FromRoute] Guid merchantId,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await _specialHolidayService.GetSpecialHolidaysByMerchantAsync(merchantId, includeInactive, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Belirli tarih aralığındaki özel tatilleri getir
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Özel tatil listesi</returns>
    [HttpGet("merchant/{merchantId:guid}/date-range")]
    [ProducesResponseType(typeof(List<SpecialHolidayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSpecialHolidaysByDateRange(
        [FromRoute] Guid merchantId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct = default)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { error = "Başlangıç tarihi bitiş tarihinden sonra olamaz" });
        }

        var result = await _specialHolidayService.GetSpecialHolidaysByDateRangeAsync(merchantId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gelecek özel tatilleri getir
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Gelecek özel tatil listesi</returns>
    [HttpGet("merchant/{merchantId:guid}/upcoming")]
    [ProducesResponseType(typeof(List<SpecialHolidayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingSpecialHolidays(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _specialHolidayService.GetUpcomingSpecialHolidaysAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ID'ye göre özel tatil getir
    /// </summary>
    /// <param name="id">Özel tatil ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Özel tatil detayı</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecialHolidayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpecialHolidayById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _specialHolidayService.GetSpecialHolidayByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant'ın belirli bir tarihteki durumunu kontrol et
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="checkDate">Kontrol edilecek tarih</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Merchant durumu</returns>
    [HttpGet("merchant/{merchantId:guid}/availability")]
    [ProducesResponseType(typeof(MerchantAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckMerchantAvailability(
        [FromRoute] Guid merchantId,
        [FromQuery] DateTime? checkDate = null,
        CancellationToken ct = default)
    {
        var dateToCheck = checkDate ?? DateTime.UtcNow;
        var result = await _specialHolidayService.CheckMerchantAvailabilityAsync(merchantId, dateToCheck, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni özel tatil oluştur
    /// </summary>
    /// <param name="request">Özel tatil bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Oluşturulan özel tatil</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(SpecialHolidayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSpecialHoliday(
        [FromBody] CreateSpecialHolidayRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _specialHolidayService.CreateSpecialHolidayAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Özel tatil güncelle
    /// </summary>
    /// <param name="id">Özel tatil ID</param>
    /// <param name="request">Güncellenmiş bilgiler</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Güncellenmiş özel tatil</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(SpecialHolidayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSpecialHoliday(
        [FromRoute] Guid id,
        [FromBody] UpdateSpecialHolidayRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _specialHolidayService.UpdateSpecialHolidayAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Özel tatil sil
    /// </summary>
    /// <param name="id">Özel tatil ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSpecialHoliday(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _specialHolidayService.DeleteSpecialHolidayAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Özel tatili aktif/pasif yap
    /// </summary>
    /// <param name="id">Özel tatil ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleSpecialHolidayStatus(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _specialHolidayService.ToggleSpecialHolidayStatusAsync(id, userId, ct);
        return ToActionResult(result);
    }
}

