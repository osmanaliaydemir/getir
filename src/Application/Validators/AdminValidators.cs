using FluentValidation;
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
            .MaximumLength(500)
            .WithMessage("Comments cannot exceed 500 characters")
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
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters");

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
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters");

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
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Action)
            .MaximumLength(100)
            .WithMessage("Action cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Action));

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .WithMessage("Entity type cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.EntityType));
    }
}

public class AdminSearchQueryValidator : AbstractValidator<AdminSearchQuery>
{
    public AdminSearchQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .WithMessage("Entity type cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.EntityType));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .WithMessage("Status cannot exceed 50 characters")
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
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");
    }
}
