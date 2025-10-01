using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.RevieweeId)
            .NotEmpty()
            .WithMessage("Reviewee ID is required");

        RuleFor(x => x.RevieweeType)
            .NotEmpty()
            .WithMessage("Reviewee type is required")
            .Must(type => type.ToLower() == "merchant" || type.ToLower() == "courier")
            .WithMessage("Reviewee type must be 'Merchant' or 'Courier'");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required")
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Maximum 10 tags allowed")
            .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && tag.Length <= 50))
            .WithMessage("Each tag must be between 1 and 50 characters");
    }
}

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required")
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Maximum 10 tags allowed")
            .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && tag.Length <= 50))
            .WithMessage("Each tag must be between 1 and 50 characters");
    }
}

public class ReviewModerationRequestValidator : AbstractValidator<ReviewModerationRequest>
{
    public ReviewModerationRequestValidator()
    {
        RuleFor(x => x.ModerationNotes)
            .MaximumLength(500)
            .WithMessage("Moderation notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ModerationNotes));
    }
}

public class ReviewHelpfulRequestValidator : AbstractValidator<ReviewHelpfulRequest>
{
    public ReviewHelpfulRequestValidator()
    {
        RuleFor(x => x.IsHelpful)
            .NotNull()
            .WithMessage("Helpful vote is required");
    }
}

public class ReviewSearchQueryValidator : AbstractValidator<ReviewSearchQuery>
{
    public ReviewSearchQueryValidator()
    {
        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Minimum rating must be between 1 and 5")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Maximum rating must be between 1 and 5")
            .When(x => x.MaxRating.HasValue);

        RuleFor(x => x.MaxRating)
            .GreaterThanOrEqualTo(x => x.MinRating)
            .WithMessage("Maximum rating must be greater than or equal to minimum rating")
            .When(x => x.MinRating.HasValue && x.MaxRating.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.RevieweeType)
            .Must(type => type == null || type.ToLower() == "merchant" || type.ToLower() == "courier")
            .WithMessage("Reviewee type must be 'Merchant' or 'Courier'")
            .When(x => !string.IsNullOrEmpty(x.RevieweeType));
    }
}

public class RatingCalculationRequestValidator : AbstractValidator<RatingCalculationRequest>
{
    public RatingCalculationRequestValidator()
    {
        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("Entity ID is required");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .Must(type => type.ToLower() == "merchant" || type.ToLower() == "courier")
            .WithMessage("Entity type must be 'Merchant' or 'Courier'");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
