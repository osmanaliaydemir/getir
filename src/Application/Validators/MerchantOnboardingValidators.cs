using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateMerchantOnboardingRequestValidator : AbstractValidator<CreateMerchantOnboardingRequest>
{
    public CreateMerchantOnboardingRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");
    }
}

public class UpdateOnboardingStepRequestValidator : AbstractValidator<UpdateOnboardingStepRequest>
{
    public UpdateOnboardingStepRequestValidator()
    {
        RuleFor(x => x.OnboardingId)
            .NotEmpty().WithMessage("Onboarding ID is required");

        RuleFor(x => x.StepName)
            .NotEmpty().WithMessage("Step name is required")
            .Must(BeValidStepName).WithMessage("Invalid step name");

        RuleFor(x => x.IsCompleted)
            .NotNull().WithMessage("IsCompleted is required");
    }

    private static bool BeValidStepName(string stepName)
    {
        var validSteps = new[] { "BasicInfo", "BusinessInfo", "WorkingHours", "DeliveryZones", "Products", "Documents" };
        return validSteps.Contains(stepName, StringComparer.OrdinalIgnoreCase);
    }
}

public class CompleteOnboardingRequestValidator : AbstractValidator<CompleteOnboardingRequest>
{
    public CompleteOnboardingRequestValidator()
    {
        RuleFor(x => x.OnboardingId)
            .NotEmpty().WithMessage("Onboarding ID is required");
    }
}

public class ApproveMerchantRequestValidator : AbstractValidator<ApproveMerchantRequest>
{
    public ApproveMerchantRequestValidator()
    {
        RuleFor(x => x.OnboardingId)
            .NotEmpty().WithMessage("Onboarding ID is required");

        RuleFor(x => x.IsApproved)
            .NotNull().WithMessage("IsApproved is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when not approved")
            .When(x => !x.IsApproved);
    }
}
