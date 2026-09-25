using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Services.Interfaces;

public interface INotificationService
{
    Task SendVerificationOtpsAsync(User user, string emailOtp, string phoneOtp);
    Task SendEmailOtpAsync(User user, string otp);
    Task SendPhoneOtpAsync(User user, string otp);
    Task SendWelcomeEmailAsync(User user);
    Task SendAccountVerifiedEmailAsync(User user);
    Task SendLoginAlertAsync(User user);
    Task SendPasswordChangedAsync(User user);
    Task SendPasswordResetOtpAsync(User user, string otp);
    Task SendProfileUpdatedEmailAsync(User user);
    Task SendAccountDeactivatedAsync(User user);
    Task SendRideStatusAsync(User passenger, User? driver, Ride ride, string status);
    Task SendDriverApprovedAsync(User driver);
    Task SendDriverRejectedAsync(User driver, string? reason);
}
