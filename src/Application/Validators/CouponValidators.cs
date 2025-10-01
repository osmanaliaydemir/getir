using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters")
            .Matches("^[A-Z0-9]+$").WithMessage("Code must contain only uppercase letters and numbers");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .Must(x => new[] { "Percentage", "FixedAmount" }.Contains(x))
            .WithMessage("Discount type must be Percentage or FixedAmount");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount must be positive");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
