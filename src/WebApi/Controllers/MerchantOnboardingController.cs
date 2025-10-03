using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant onboarding controller for merchant onboarding operations
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
    /// Get onboarding status
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onboarding status</returns>
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
    /// Get onboarding progress
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onboarding progress</returns>
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
    /// Get onboarding steps
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onboarding steps</returns>
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
    /// Complete onboarding step
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="stepId">Step ID</param>
    /// <param name="request">Complete step request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated step</returns>
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

        return BadRequest("Complete onboarding step functionality not implemented yet");
    }

    /// <summary>
    /// Submit onboarding for review
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitOnboarding(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        return BadRequest("Submit onboarding functionality not implemented yet");
    }

    /// <summary>
    /// Get onboarding checklist
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onboarding checklist</returns>
    [HttpGet("checklist")]
    [ProducesResponseType(typeof(OnboardingChecklistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingChecklist(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        return BadRequest("Get onboarding checklist functionality not implemented yet");
    }
}
