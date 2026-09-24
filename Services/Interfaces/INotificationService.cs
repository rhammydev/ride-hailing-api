using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Services.Interfaces;

public interface INotificationService
{
    Task SendOtpEmailAsync(string email, string fullName, string otp);
    Task SendOtpSmsAsync(string? recipient, string otp);
    Task SendWelcomeEmailAsync(string email, string fullName, string userId);
    Task SendAccountVerifiedEmailAsync(string email, string fullName, string userId);
    Task SendLoginAlertAsync(string email, string fullName);
    Task SendPasswordChangedAsync(string email, string fullName, string? phoneNumber);
    Task SendPasswordResetOtpAsync(string email, string fullName, string? phoneNumber, string otp);
    Task SendProfileUpdatedEmailAsync(string email, string fullName);
    Task SendAccountDeactivatedAsync(string email, string fullName, string? phoneNumber);
    Task SendRideStatusAsync(User passenger, User? driver, Ride ride, string status);
    Task SendDriverApprovedAsync(User driver);
    Task SendDriverRejectedAsync(User driver, string? reason);
}
