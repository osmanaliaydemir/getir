using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Mağaza onboarding işlemleri için mağaza onboarding controller'ı
/// </summary>
[ApiController]
[Route("api/v1/merchants/{merchantId:guid}/[controller]")]
[Tags("Merchant Onboarding")]
[Authorize]
public class MerchantOnboardingController : BaseController
{
    private readonly IMerchantOnboardingService _merchantOnboardingService;

    public MerchantOnboardingController(IMerchantOnboardingService merchantOnboardingService)
    {
        _merchantOnboardingService = merchantOnboardingService;
    }

    /// <summary>
    /// Onboarding durumunu getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Onboarding durumu</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MerchantOnboardingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingStatus(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantOnboardingService.GetOnboardingStatusAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Onboarding ilerlemesini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Onboarding ilerlemesi</returns>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(OnboardingProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingProgress(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantOnboardingService.GetOnboardingProgressAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Onboarding adımlarını getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Onboarding adımları</returns>
    [HttpGet("steps")]
    [ProducesResponseType(typeof(IEnumerable<OnboardingStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingSteps(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantOnboardingService.GetOnboardingStepsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Onboarding adımını tamamla
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="stepId">Adım ID</param>
    /// <param name="request">Adım tamamlama isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen adım</returns>
    [HttpPost("steps/{stepId}/complete")]
    [ProducesResponseType(typeof(OnboardingStepResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOnboardingStep(
        [FromRoute] Guid merchantId,
        [FromRoute] Guid stepId,
        [FromBody] CompleteOnboardingStepRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _merchantOnboardingService.CompleteOnboardingStepAsync(merchantId, stepId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Onboarding'i incelemeye gönder
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitOnboarding(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantOnboardingService.SubmitOnboardingAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Onboarding kontrol listesini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Onboarding kontrol listesi</returns>
    [HttpGet("checklist")]
    [ProducesResponseType(typeof(OnboardingChecklistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingChecklist(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantOnboardingService.GetOnboardingChecklistAsync(merchantId, ct);
        return ToActionResult(result);
    }
}
