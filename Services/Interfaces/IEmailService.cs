namespace Ride_Hailing_API.Services.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string fullName, string otp);
    Task SendWelcomeAsync(string toEmail, string fullName, string userId);
    Task SendAccountVerifiedAsync(string toEmail, string fullName, string userId);
    Task SendLoginAlertAsync(string toEmail, string fullName);
    Task SendPasswordChangedAsync(string toEmail, string fullName);
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp);
    Task SendProfileUpdatedAsync(string toEmail, string fullName);
    Task SendAccountDeactivatedAsync(string toEmail, string fullName);
    Task SendRideStatusAsync(string toEmail, string fullName, string subject, string rideReference,
        string pickupLocation, string destination, string status, string? cancellationReason);
    Task SendDriverApprovedAsync(string toEmail, string fullName);
    Task SendDriverRejectedAsync(string toEmail, string fullName, string? reason);
}
