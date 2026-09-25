namespace Ride_Hailing_API.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpAsync(string toEmail, string fullName, string otp);
    Task<bool> SendWelcomeAsync(string toEmail, string fullName, string userId);
    Task<bool> SendAccountVerifiedAsync(string toEmail, string fullName, string userId);
    Task<bool> SendLoginAlertAsync(string toEmail, string fullName);
    Task<bool> SendPasswordChangedAsync(string toEmail, string fullName);
    Task<bool> SendPasswordResetOtpAsync(string toEmail, string fullName, string otp);
    Task<bool> SendProfileUpdatedAsync(string toEmail, string fullName);
    Task<bool> SendAccountDeactivatedAsync(string toEmail, string fullName);
    Task<bool> SendRideStatusAsync(string toEmail, string fullName, string subject, string rideReference,
        string pickupLocation, string destination, string status, string? cancellationReason);
    Task<bool> SendDriverApprovedAsync(string toEmail, string fullName);
    Task<bool> SendDriverRejectedAsync(string toEmail, string fullName, string? reason);
}
