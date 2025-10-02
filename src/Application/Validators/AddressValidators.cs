using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Title must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.FullAddress)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(ApplicationConstants.MaxAddressLength).WithMessage($"Address must not exceed {ApplicationConstants.MaxAddressLength} characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"City must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"District must not exceed {ApplicationConstants.MaxNameLength} characters");
    }
}

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Title must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.FullAddress)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(ApplicationConstants.MaxAddressLength).WithMessage($"Address must not exceed {ApplicationConstants.MaxAddressLength} characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required");
    }
}
