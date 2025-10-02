using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateServiceCategoryRequestValidator : AbstractValidator<CreateServiceCategoryRequest>
{
    public CreateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service category name is required")
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Image URL must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.IconUrl)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Icon URL must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be positive");
    }
}

public class UpdateServiceCategoryRequestValidator : AbstractValidator<UpdateServiceCategoryRequest>
{
    public UpdateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service category name is required")
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Image URL must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.IconUrl)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Icon URL must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be positive");
    }
}
