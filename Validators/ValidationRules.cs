using FluentValidation;

namespace Ride_Hailing_API.Validators;

public static class ValidationRules
{
    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,100}$";
    public const string PhonePattern = @"^\+?\d{10,15}$";
    public const string OtpPattern = @"^\d{6}$";

    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s'-]+$").WithMessage("{PropertyName} contains invalid characters.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

    public static IRuleBuilderOptions<T, string> ValidPhone<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Phone number is required.")
            .Matches(PhonePattern).WithMessage("Phone number must contain 10-15 digits and may start with '+'.");

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Password is required.")
            .Matches(PasswordPattern)
            .WithMessage("{PropertyName} must be 8-100 characters and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

    public static IRuleBuilderOptions<T, string> ValidOtp<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("OTP code is required.")
            .Matches(OtpPattern).WithMessage("OTP code must be exactly 6 digits.");
}
