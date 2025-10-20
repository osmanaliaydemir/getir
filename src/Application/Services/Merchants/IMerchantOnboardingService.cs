using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Merchant onboarding servisi: adım adım onboarding süreci yönetimi, ilerleme takibi, admin onayı.
/// </summary>
public interface IMerchantOnboardingService
{
    // Onboarding Management
    /// <summary>Yeni onboarding oluşturur (6 adımlı süreç).</summary>
    Task<Result<MerchantOnboardingResponse>> CreateOnboardingAsync(CreateMerchantOnboardingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Onboarding durumunu getirir.</summary>
    Task<Result<MerchantOnboardingResponse>> GetOnboardingStatusAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Onboarding ilerlemesini getirir (tamamlanan adımlar, mevcut adım).</summary>
    Task<Result<OnboardingProgressResponse>> GetOnboardingProgressAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Onboarding adımını günceller (ilerleme hesaplar).</summary>
    Task<Result<MerchantOnboardingResponse>> UpdateOnboardingStepAsync(UpdateOnboardingStepRequest request, CancellationToken cancellationToken = default);

    /// <summary>Onboarding'i tamamlar (tüm adımlar tamamlanmalı).</summary>
    Task<Result> CompleteOnboardingAsync(CompleteOnboardingRequest request, CancellationToken cancellationToken = default);

    // Admin Functions
    /// <summary>Admin onayı bekleyen merchantları getirir.</summary>
    Task<Result<PagedResult<MerchantOnboardingResponse>>> GetPendingApprovalsAsync(GetPendingApprovalsQuery query, CancellationToken cancellationToken = default);

    /// <summary>Merchant'ı onaylar veya reddeder.</summary>
    Task<Result> ApproveMerchantAsync(ApproveMerchantRequest request, CancellationToken cancellationToken = default);

    // Helper Methods
    /// <summary>Onboarding adımlarını getirir.</summary>
    Task<Result<List<OnboardingStepResponse>>> GetOnboardingStepsAsync(CancellationToken cancellationToken = default);

    /// <summary>Merchant'ın satış yapabilir durumda olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> CanMerchantStartTradingAsync(Guid merchantId, CancellationToken cancellationToken = default);

    // Additional methods for controller
    /// <summary>Onboarding adımını tamamlar.</summary>
    Task<Result<OnboardingStepResponse>> CompleteOnboardingStepAsync(Guid merchantId, Guid stepId, CompleteOnboardingStepRequest request, CancellationToken cancellationToken = default);

    /// <summary>Onboarding'i gönderir (admin onayına).</summary>
    Task<Result> SubmitOnboardingAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Onboarding kontrol listesini getirir.</summary>
    Task<Result<OnboardingChecklistResponse>> GetOnboardingChecklistAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
