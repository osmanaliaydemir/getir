using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateMerchantRequestValidator : AbstractValidator<CreateMerchantRequest>
{
    public CreateMerchantRequestValidator()
    {
        RuleFor(x => x.Name).NameRule("Merchant name");
        RuleFor(x => x.ServiceCategoryId).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.Address).AddressRule("Address");
        RuleFor(x => x.PhoneNumber).PhoneNumberRule();
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(ApplicationConstants.MaxEmailLength).WithMessage($"Email must not exceed {ApplicationConstants.MaxEmailLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.MinimumOrderAmount).PriceRule("Minimum order amount");
        RuleFor(x => x.DeliveryFee).PriceRule("Delivery fee");
    }
}

public class UpdateMerchantRequestValidator : AbstractValidator<UpdateMerchantRequest>
{
    public UpdateMerchantRequestValidator()
    {
        RuleFor(x => x.Name).NameRule("Merchant name");
        RuleFor(x => x.Address).AddressRule("Address");
        RuleFor(x => x.PhoneNumber).PhoneNumberRule();
        RuleFor(x => x.MinimumOrderAmount).PriceRule("Minimum order amount");
        RuleFor(x => x.DeliveryFee).PriceRule("Delivery fee");
    }
}
