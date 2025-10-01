using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).WithMessage("Discounted price must be less than regular price")
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero")
            .When(x => x.DiscountedPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be positive");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).WithMessage("Discounted price must be less than regular price")
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero")
            .When(x => x.DiscountedPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be positive");
    }
}
