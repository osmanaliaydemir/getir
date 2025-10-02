using FluentValidation;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class MerchantApprovalRequestValidator : AbstractValidator<MerchantApprovalRequest>
{
    public MerchantApprovalRequestValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required");

        RuleFor(x => x.Comments)
            .MaximumLength(ApplicationConstants.MaxCommentLength)
            .WithMessage($"Comments cannot exceed {ApplicationConstants.MaxCommentLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));

        RuleFor(x => x.Comments)
            .NotEmpty()
            .WithMessage("Comments are required when rejecting an application")
            .When(x => !x.IsApproved);
    }
}

public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequest>
{
    public AdminCreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"First name cannot exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Last name cannot exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(ApplicationConstants.MaxEmailLength)
            .WithMessage($"Email cannot exceed {ApplicationConstants.MaxEmailLength} characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(ApplicationConstants.MaxPhoneNumberLength)
            .WithMessage($"Phone cannot exceed {ApplicationConstants.MaxPhoneNumberLength} characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(ApplicationConstants.MinPasswordLength)
            .WithMessage($"Password must be at least {ApplicationConstants.MinPasswordLength} characters")
            .MaximumLength(ApplicationConstants.MaxPasswordLength)
            .WithMessage($"Password cannot exceed {ApplicationConstants.MaxPasswordLength} characters");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .Must(role => new[] { "Admin", "MerchantOwner", "Courier", "Customer" }.Contains(role))
            .WithMessage("Invalid role. Must be Admin, MerchantOwner, Courier, or Customer");
    }
}

public class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
{
    public AdminUpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"First name cannot exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Last name cannot exceed {ApplicationConstants.MaxNameLength} characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(ApplicationConstants.MaxEmailLength)
            .WithMessage($"Email cannot exceed {ApplicationConstants.MaxEmailLength} characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(ApplicationConstants.MaxPhoneNumberLength)
            .WithMessage($"Phone cannot exceed {ApplicationConstants.MaxPhoneNumberLength} characters");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .Must(role => new[] { "Admin", "MerchantOwner", "Courier", "Customer" }.Contains(role))
            .WithMessage("Invalid role. Must be Admin, MerchantOwner, Courier, or Customer");
    }
}

public class AuditLogQueryValidator : AbstractValidator<AuditLogQuery>
{
    public AuditLogQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .WithMessage("From date must be less than or equal to to date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ApplicationConstants.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {ApplicationConstants.MaxPageSize}");

        RuleFor(x => x.Action)
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Action cannot exceed {ApplicationConstants.MaxNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Action));

        RuleFor(x => x.EntityType)
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Entity type cannot exceed {ApplicationConstants.MaxNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.EntityType));
    }
}

public class AdminSearchQueryValidator : AbstractValidator<AdminSearchQuery>
{
    public AdminSearchQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength)
            .WithMessage($"Search term cannot exceed {ApplicationConstants.MaxDescriptionLength} characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.EntityType)
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Entity type cannot exceed {ApplicationConstants.MaxNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.EntityType));

        RuleFor(x => x.Status)
            .MaximumLength(ApplicationConstants.MaxNameLength)
            .WithMessage($"Status cannot exceed {ApplicationConstants.MaxNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .WithMessage("From date must be less than or equal to to date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ApplicationConstants.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {ApplicationConstants.MaxPageSize}");
    }
}
