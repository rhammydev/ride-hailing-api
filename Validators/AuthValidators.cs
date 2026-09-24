using FluentValidation;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.DTOs.Auth;

namespace Ride_Hailing_API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .ValidName();

        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.PhoneNumber)
            .ValidPhone();

        RuleFor(x => x.Password)
            .StrongPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.")
            .Must(r => r == UserRole.Passenger || r == UserRole.Driver)
            .WithMessage("Registration role must be either Passenger or Driver.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.OtpCode)
            .ValidOtp();
    }
}

public class VerifyPhoneRequestValidator : AbstractValidator<VerifyPhoneRequest>
{
    public VerifyPhoneRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .ValidPhone();

        RuleFor(x => x.OtpCode)
            .ValidOtp();
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.OtpCode)
            .ValidOtp();

        RuleFor(x => x.NewPassword)
            .StrongPassword();

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .StrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("New password cannot be the same as the current password.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
