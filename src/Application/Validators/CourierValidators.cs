using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class UpdateCourierLocationRequestValidator : AbstractValidator<UpdateCourierLocationRequest>
{
    public UpdateCourierLocationRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180");
    }
}

public class AcceptOrderRequestValidator : AbstractValidator<AcceptOrderRequest>
{
    public AcceptOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}

public class StartDeliveryRequestValidator : AbstractValidator<StartDeliveryRequest>
{
    public StartDeliveryRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}

public class CompleteDeliveryRequestValidator : AbstractValidator<CompleteDeliveryRequest>
{
    public CompleteDeliveryRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.DeliveryNotes)
            .MaximumLength(ApplicationConstants.MaxCommentLength).WithMessage($"Delivery notes must not exceed {ApplicationConstants.MaxCommentLength} characters");
    }
}

public class AssignOrderRequestValidator : AbstractValidator<AssignOrderRequest>
{
    public AssignOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}

public class FindNearestCouriersRequestValidator : AbstractValidator<FindNearestCouriersRequest>
{
    public FindNearestCouriersRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.MaxDistanceKm)
            .GreaterThan(0).WithMessage("Max distance must be greater than 0")
            .LessThanOrEqualTo(ApplicationConstants.MaxQuantity).WithMessage($"Max distance cannot exceed {ApplicationConstants.MaxQuantity} km");

        RuleFor(x => x.MaxCouriers)
            .GreaterThan(0).WithMessage("Max couriers must be greater than 0")
            .LessThanOrEqualTo(ApplicationConstants.MaxRecentItems).WithMessage($"Max couriers cannot exceed {ApplicationConstants.MaxRecentItems}");
    }
}

public class CourierEarningsQueryValidator : AbstractValidator<CourierEarningsQuery>
{
    public CourierEarningsQueryValidator()
    {
        RuleFor(x => x.CourierId)
            .NotEmpty().WithMessage("Courier ID is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
