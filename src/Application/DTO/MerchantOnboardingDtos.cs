using Getir.Application.Common;

namespace Getir.Application.DTO;

// Request DTOs
public record CreateMerchantOnboardingRequest(
    Guid MerchantId,
    Guid OwnerId);

public record UpdateOnboardingStepRequest(
    Guid OnboardingId,
    string StepName,
    bool IsCompleted);

public record CompleteOnboardingRequest(
    Guid OnboardingId);

public record ApproveMerchantRequest(
    Guid OnboardingId,
    bool IsApproved,
    string? RejectionReason = null);

// Response DTOs
public record MerchantOnboardingResponse(
    Guid Id,
    Guid MerchantId,
    Guid OwnerId,
    bool BasicInfoCompleted,
    bool BusinessInfoCompleted,
    bool WorkingHoursCompleted,
    bool DeliveryZonesCompleted,
    bool ProductsAdded,
    bool DocumentsUploaded,
    bool IsVerified,
    bool IsApproved,
    string? RejectionReason,
    DateTime? VerifiedAt,
    DateTime? ApprovedAt,
    int CompletedSteps,
    int TotalSteps,
    decimal ProgressPercentage,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record OnboardingStepResponse(
    string StepName,
    string DisplayName,
    string Description,
    bool IsCompleted,
    bool IsRequired,
    string? NextStep = null);

public record OnboardingProgressResponse(
    Guid OnboardingId,
    Guid MerchantId,
    List<OnboardingStepResponse> Steps,
    int CompletedSteps,
    int TotalSteps,
    decimal ProgressPercentage,
    string CurrentStep,
    bool CanProceed,
    bool IsCompleted,
    bool IsApproved);

// Query DTOs
public record GetOnboardingStatusQuery(
    Guid MerchantId);

public record GetPendingApprovalsQuery(
    PaginationQuery Pagination);

public record CompleteOnboardingStepRequest(
    string StepName,
    Dictionary<string, string>? Data = null);

public record OnboardingChecklistResponse(
    Guid MerchantId,
    List<OnboardingChecklistItem> Items,
    int CompletedCount,
    int TotalCount,
    decimal CompletionPercentage);

public record OnboardingChecklistItem(
    string Name,
    string Description,
    bool IsCompleted,
    bool IsRequired);