using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant is required");

        RuleFor(x => x.Name).NameRule("Product name");
        RuleFor(x => x.Price).PriceRule("Price");
        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).WithMessage("Discounted price must be less than regular price")
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero")
            .When(x => x.DiscountedPrice.HasValue);
        RuleFor(x => x.StockQuantity).QuantityRule("Stock quantity");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NameRule("Product name");
        RuleFor(x => x.Price).PriceRule("Price");
        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).WithMessage("Discounted price must be less than regular price")
            .GreaterThan(0).WithMessage("Discounted price must be greater than zero")
            .When(x => x.DiscountedPrice.HasValue);
        RuleFor(x => x.StockQuantity).QuantityRule("Stock quantity");
    }
}

// Merchant-specific validators
public class UpdateProductStockRequestValidator : AbstractValidator<UpdateProductStockRequest>
{
    public UpdateProductStockRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewStockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be positive");
    }
}

public class UpdateProductOrderRequestValidator : AbstractValidator<UpdateProductOrderRequest>
{
    public UpdateProductOrderRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.NewDisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be positive");
    }
}

public class BulkUpdateProductOrderRequestValidator : AbstractValidator<BulkUpdateProductOrderRequest>
{
    public BulkUpdateProductOrderRequestValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Products list cannot be empty");

        RuleForEach(x => x.Products)
            .SetValidator(new UpdateProductOrderRequestValidator());
    }
}

public class ToggleProductAvailabilityRequestValidator : AbstractValidator<ToggleProductAvailabilityRequest>
{
    public ToggleProductAvailabilityRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.IsAvailable)
            .NotNull().WithMessage("Availability status is required");
    }
}