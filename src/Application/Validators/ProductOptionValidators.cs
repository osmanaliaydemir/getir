using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateProductOptionGroupRequestValidator : AbstractValidator<CreateProductOptionGroupRequest>
{
    public CreateProductOptionGroupRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.MinSelection)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum selection must be 0 or greater");

        RuleFor(x => x.MaxSelection)
            .GreaterThan(0).WithMessage("Maximum selection must be greater than 0")
            .GreaterThanOrEqualTo(x => x.MinSelection).WithMessage("Maximum selection must be greater than or equal to minimum selection");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}

public class UpdateProductOptionGroupRequestValidator : AbstractValidator<UpdateProductOptionGroupRequest>
{
    public UpdateProductOptionGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.MinSelection)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum selection must be 0 or greater");

        RuleFor(x => x.MaxSelection)
            .GreaterThan(0).WithMessage("Maximum selection must be greater than 0")
            .GreaterThanOrEqualTo(x => x.MinSelection).WithMessage("Maximum selection must be greater than or equal to minimum selection");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}

public class CreateProductOptionRequestValidator : AbstractValidator<CreateProductOptionRequest>
{
    public CreateProductOptionRequestValidator()
    {
        RuleFor(x => x.ProductOptionGroupId)
            .NotEmpty().WithMessage("Product option group ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.ExtraPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Extra price must be 0 or greater");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}

public class UpdateProductOptionRequestValidator : AbstractValidator<UpdateProductOptionRequest>
{
    public UpdateProductOptionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"Name must not exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"Description must not exceed {ApplicationConstants.MaxDescriptionLength} characters");

        RuleFor(x => x.ExtraPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Extra price must be 0 or greater");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}

public class BulkCreateProductOptionsRequestValidator : AbstractValidator<BulkCreateProductOptionsRequest>
{
    public BulkCreateProductOptionsRequestValidator()
    {
        RuleFor(x => x.ProductOptionGroupId)
            .NotEmpty().WithMessage("Product option group ID is required");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options list cannot be empty");

        RuleForEach(x => x.Options)
            .SetValidator(new CreateProductOptionRequestValidator());
    }
}

public class BulkUpdateProductOptionsRequestValidator : AbstractValidator<BulkUpdateProductOptionsRequest>
{
    public BulkUpdateProductOptionsRequestValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options list cannot be empty");

        RuleForEach(x => x.Options)
            .SetValidator(new UpdateProductOptionRequestValidator());
    }
}

public class CreateOrderLineOptionRequestValidator : AbstractValidator<CreateOrderLineOptionRequest>
{
    public CreateOrderLineOptionRequestValidator()
    {
        RuleFor(x => x.ProductOptionId)
            .NotEmpty().WithMessage("Product option ID is required");
    }
}
