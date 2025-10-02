using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

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
            .MaximumLength(ApplicationConstants.MaxAddressLength).WithMessage($"Address must not exceed {ApplicationConstants.MaxAddressLength} characters");

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
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(ApplicationConstants.MaxQuantity).WithMessage($"Quantity must not exceed {ApplicationConstants.MaxQuantity}");
    }
}

// Merchant-specific validators
public class RejectOrderRequestValidator : AbstractValidator<RejectOrderRequest>
{
    public RejectOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Reason)
            .MaximumLength(ApplicationConstants.MaxCommentLength).WithMessage($"Reason must not exceed {ApplicationConstants.MaxCommentLength} characters");
    }
}

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(ApplicationConstants.MaxCommentLength).WithMessage($"Reason must not exceed {ApplicationConstants.MaxCommentLength} characters");
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Invalid order status");

        RuleFor(x => x.Reason)
            .MaximumLength(ApplicationConstants.MaxCommentLength).WithMessage($"Reason must not exceed {ApplicationConstants.MaxCommentLength} characters");
    }

    private static bool BeValidStatus(string status)
    {
        try
        {
            OrderStatusExtensions.FromString(status);
            return true;
        }
        catch
        {
            return false;
        }
    }
}