using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count > 0).WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderLineRequestValidator());

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(pm => new[] { "CreditCard", "Cash", "Wallet" }.Contains(pm))
            .WithMessage("Invalid payment method");
    }
}

public class OrderLineRequestValidator : AbstractValidator<OrderLineRequest>
{
    public OrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");
    }
}
