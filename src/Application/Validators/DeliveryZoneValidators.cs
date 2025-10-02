using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateDeliveryZoneRequestValidator : AbstractValidator<CreateDeliveryZoneRequest>
{
    public CreateDeliveryZoneRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zone name is required")
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("Delivery fee must be non-negative");

        RuleFor(x => x.EstimatedDeliveryTime)
            .GreaterThan(0).WithMessage("Estimated delivery time must be positive");

        RuleFor(x => x.Points)
            .NotEmpty().WithMessage("Points are required")
            .Must(points => points.Count >= 3).WithMessage("At least 3 points are required for a delivery zone");

        RuleForEach(x => x.Points)
            .SetValidator(new DeliveryZonePointRequestValidator());
    }
}

public class UpdateDeliveryZoneRequestValidator : AbstractValidator<UpdateDeliveryZoneRequest>
{
    public UpdateDeliveryZoneRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zone name is required")
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("Delivery fee must be non-negative");

        RuleFor(x => x.EstimatedDeliveryTime)
            .GreaterThan(0).WithMessage("Estimated delivery time must be positive");

        RuleFor(x => x.Points)
            .NotEmpty().WithMessage("Points are required")
            .Must(points => points.Count >= 3).WithMessage("At least 3 points are required for a delivery zone");

        RuleForEach(x => x.Points)
            .SetValidator(new DeliveryZonePointRequestValidator());
    }
}

public class DeliveryZonePointRequestValidator : AbstractValidator<DeliveryZonePointRequest>
{
    public DeliveryZonePointRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

public class CheckDeliveryZoneRequestValidator : AbstractValidator<CheckDeliveryZoneRequest>
{
    public CheckDeliveryZoneRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180");
    }
}
