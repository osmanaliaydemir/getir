using FluentValidation;
using Getir.Application.Common;

namespace Getir.Application.Validators;

/// <summary>
/// Common validation rules as extension methods
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Email validation rule
    /// </summary>
    public static IRuleBuilder<T, string> EmailRule<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(ApplicationConstants.MaxEmailLength).WithMessage($"Email must not exceed {ApplicationConstants.MaxEmailLength} characters");
    }

    /// <summary>
    /// Email validation rule for nullable strings
    /// </summary>
    public static IRuleBuilder<T, string?> EmailRuleNullable<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(ApplicationConstants.MaxEmailLength).WithMessage($"Email must not exceed {ApplicationConstants.MaxEmailLength} characters")
            .When(x => !string.IsNullOrEmpty(x as string));
    }

    /// <summary>
    /// Name validation rule
    /// </summary>
    public static IRuleBuilder<T, string> NameRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(ApplicationConstants.MaxNameLength).WithMessage($"{fieldName} must not exceed {ApplicationConstants.MaxNameLength} characters");
    }

    /// <summary>
    /// Description validation rule
    /// </summary>
    public static IRuleBuilder<T, string?> DescriptionRule<T>(this IRuleBuilder<T, string?> rule, string fieldName)
    {
        return rule
            .MaximumLength(ApplicationConstants.MaxDescriptionLength).WithMessage($"{fieldName} must not exceed {ApplicationConstants.MaxDescriptionLength} characters")
            .When(x => !string.IsNullOrEmpty(x as string));
    }

    /// <summary>
    /// Phone number validation rule
    /// </summary>
    public static IRuleBuilder<T, string?> PhoneNumberRule<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .MaximumLength(ApplicationConstants.MaxPhoneNumberLength).WithMessage($"Phone number must not exceed {ApplicationConstants.MaxPhoneNumberLength} characters")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x as string));
    }

    /// <summary>
    /// Password validation rule
    /// </summary>
    public static IRuleBuilder<T, string> PasswordRule<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(ApplicationConstants.MinPasswordLength).WithMessage($"Password must be at least {ApplicationConstants.MinPasswordLength} characters")
            .MaximumLength(ApplicationConstants.MaxPasswordLength).WithMessage($"Password must not exceed {ApplicationConstants.MaxPasswordLength} characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");
    }

    /// <summary>
    /// Price validation rule
    /// </summary>
    public static IRuleBuilder<T, decimal> PriceRule<T>(this IRuleBuilder<T, decimal> rule, string fieldName)
    {
        return rule
            .GreaterThan(0).WithMessage($"{fieldName} must be greater than zero")
            .LessThan(ApplicationConstants.MaxQuantity).WithMessage($"{fieldName} must be less than {ApplicationConstants.MaxQuantity}");
    }

    /// <summary>
    /// Quantity validation rule
    /// </summary>
    public static IRuleBuilder<T, int> QuantityRule<T>(this IRuleBuilder<T, int> rule, string fieldName)
    {
        return rule
            .GreaterThan(0).WithMessage($"{fieldName} must be greater than zero")
            .LessThanOrEqualTo(ApplicationConstants.MaxQuantity).WithMessage($"{fieldName} must not exceed {ApplicationConstants.MaxQuantity}");
    }

    /// <summary>
    /// Rating validation rule
    /// </summary>
    public static IRuleBuilder<T, int> RatingRule<T>(this IRuleBuilder<T, int> rule)
    {
        return rule
            .InclusiveBetween(ApplicationConstants.MinRating, ApplicationConstants.MaxRating).WithMessage($"Rating must be between {ApplicationConstants.MinRating} and {ApplicationConstants.MaxRating}");
    }

    /// <summary>
    /// Address validation rule
    /// </summary>
    public static IRuleBuilder<T, string> AddressRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(ApplicationConstants.MaxAddressLength).WithMessage($"{fieldName} must not exceed {ApplicationConstants.MaxAddressLength} characters");
    }

    /// <summary>
    /// Comment validation rule
    /// </summary>
    public static IRuleBuilder<T, string> CommentRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(ApplicationConstants.MaxCommentLength).WithMessage($"{fieldName} must not exceed {ApplicationConstants.MaxCommentLength} characters");
    }
}
