using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Merchants;

public interface IMerchantOnboardingService
{
    // Onboarding Management
    Task<Result<MerchantOnboardingResponse>> CreateOnboardingAsync(
        CreateMerchantOnboardingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<MerchantOnboardingResponse>> GetOnboardingStatusAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    Task<Result<OnboardingProgressResponse>> GetOnboardingProgressAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    Task<Result<MerchantOnboardingResponse>> UpdateOnboardingStepAsync(
        UpdateOnboardingStepRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> CompleteOnboardingAsync(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken = default);

    // Admin Functions
    Task<Result<PagedResult<MerchantOnboardingResponse>>> GetPendingApprovalsAsync(
        GetPendingApprovalsQuery query,
        CancellationToken cancellationToken = default);

    Task<Result> ApproveMerchantAsync(
        ApproveMerchantRequest request,
        CancellationToken cancellationToken = default);

    // Helper Methods
    Task<Result<List<OnboardingStepResponse>>> GetOnboardingStepsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<bool>> CanMerchantStartTradingAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);
}
