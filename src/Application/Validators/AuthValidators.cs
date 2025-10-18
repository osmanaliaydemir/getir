using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).EmailRule();
        RuleFor(x => x.Password).PasswordRule();
        RuleFor(x => x.FirstName).NameRule("First name");
        RuleFor(x => x.LastName).NameRule("Last name");
        RuleFor(x => x.PhoneNumber).PhoneNumberRule();
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).EmailRule();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).EmailRule();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required")
            .MinimumLength(10).WithMessage("Invalid reset token format");

        RuleFor(x => x.NewPassword).PasswordRule();
    }
}