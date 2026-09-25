using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;
using Ride_Hailing_API.Utilities;

namespace Ride_Hailing_API.Services.Implementations;

public class NotificationService(
    IEmailService email,
    ISmsService sms,
    INotificationRepository notificationRepository,
    ILogger<NotificationService> logger) : INotificationService
{
    private const string MaskedOtp = "******";

    public Task SendVerificationOtpsAsync(User user, string emailOtp, string phoneOtp) =>
        DispatchAsync(
            Email(user, "Email verification OTP", () => email.SendOtpAsync(user.Email!, FullName(user), emailOtp)),
            Sms(user, SmsUtils.Otp(MaskedOtp), () => sms.SendSmsAsync(user.PhoneNumber, SmsUtils.Otp(phoneOtp))));

    public Task SendEmailOtpAsync(User user, string otp) =>
        DispatchAsync(Email(user, "Email verification OTP", () => email.SendOtpAsync(user.Email!, FullName(user), otp)));

    public Task SendPhoneOtpAsync(User user, string otp) =>
        DispatchAsync(Sms(user, SmsUtils.Otp(MaskedOtp), () => sms.SendSmsAsync(user.PhoneNumber, SmsUtils.Otp(otp))));

    public Task SendWelcomeEmailAsync(User user) =>
        DispatchAsync(Email(user, "Welcome to RideHail",
            () => email.SendWelcomeAsync(user.Email!, FullName(user), user.Id.ToString())));

    public Task SendAccountVerifiedEmailAsync(User user) =>
        DispatchAsync(Email(user, "Account verified",
            () => email.SendAccountVerifiedAsync(user.Email!, FullName(user), user.Id.ToString())));

    public Task SendLoginAlertAsync(User user) =>
        DispatchAsync(Email(user, "Login alert", () => email.SendLoginAlertAsync(user.Email!, FullName(user))));

    public Task SendPasswordChangedAsync(User user) =>
        DispatchAsync(
            Email(user, "Password changed", () => email.SendPasswordChangedAsync(user.Email!, FullName(user))),
            Sms(user, SmsUtils.PasswordChanged(), () => sms.SendSmsAsync(user.PhoneNumber, SmsUtils.PasswordChanged())));

    public Task SendPasswordResetOtpAsync(User user, string otp) =>
        DispatchAsync(
            Email(user, "Password reset OTP", () => email.SendPasswordResetOtpAsync(user.Email!, FullName(user), otp)),
            Sms(user, SmsUtils.PasswordResetOtp(MaskedOtp),
                () => sms.SendSmsAsync(user.PhoneNumber, SmsUtils.PasswordResetOtp(otp))));

    public Task SendProfileUpdatedEmailAsync(User user) =>
        DispatchAsync(Email(user, "Profile updated", () => email.SendProfileUpdatedAsync(user.Email!, FullName(user))));

    public Task SendAccountDeactivatedAsync(User user) =>
        DispatchAsync(
            Email(user, "Account deactivated", () => email.SendAccountDeactivatedAsync(user.Email!, FullName(user))),
            Sms(user, SmsUtils.AccountDeactivated(), () => sms.SendSmsAsync(user.PhoneNumber, SmsUtils.AccountDeactivated())));

    public Task SendRideStatusAsync(User passenger, User? driver, Ride ride, string status)
    {
        var rideRef = ride.Reference ?? ride.Id.ToString();
        var pickup = ride.PickupLocation ?? "N/A";
        var destination = ride.Destination ?? "N/A";
        var subject = $"Ride {status} - {rideRef}";
        var smsText = SmsUtils.RideStatus(rideRef, status);

        var recipients = driver == null ? new[] { passenger } : new[] { passenger, driver };
        var outgoing = recipients.SelectMany(user => new[]
        {
            Email(user, subject, () => email.SendRideStatusAsync(user.Email!, FullName(user), subject, rideRef,
                pickup, destination, status, ride.CancellationReason)),
            Sms(user, smsText, () => sms.SendSmsAsync(user.PhoneNumber, smsText))
        }).ToArray();

        return DispatchAsync(outgoing);
    }

    public Task SendDriverApprovedAsync(User driver) =>
        DispatchAsync(
            Email(driver, "Driver application approved", () => email.SendDriverApprovedAsync(driver.Email!, FullName(driver))),
            Sms(driver, SmsUtils.DriverApproved(), () => sms.SendSmsAsync(driver.PhoneNumber, SmsUtils.DriverApproved())));

    public Task SendDriverRejectedAsync(User driver, string? reason) =>
        DispatchAsync(
            Email(driver, "Driver application rejected",
                () => email.SendDriverRejectedAsync(driver.Email!, FullName(driver), reason)),
            Sms(driver, SmsUtils.DriverRejected(reason),
                () => sms.SendSmsAsync(driver.PhoneNumber, SmsUtils.DriverRejected(reason))));

    /// <summary>
    /// Sends all messages in parallel, then records each one in the Notifications table.
    /// Recording happens sequentially because the DbContext does not support concurrent operations.
    /// </summary>
    private async Task DispatchAsync(params Outgoing[] messages)
    {
        var results = await Task.WhenAll(messages.Select(m => m.Send()));

        try
        {
            for (var i = 0; i < messages.Length; i++)
            {
                var message = messages[i];
                await notificationRepository.AddAsync(new Notification
                {
                    UserId = message.User.Id,
                    Type = message.Type,
                    Recipient = message.Recipient ?? string.Empty,
                    Subject = message.Subject,
                    Message = message.LogMessage,
                    IsSent = results[i],
                    SentAt = results[i] ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await notificationRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // A failure to record a notification must not fail the business operation that triggered it.
            logger.LogError(ex, "Failed to record {Count} notification(s).", messages.Length);
        }
    }

    private static Outgoing Email(User user, string subject, Func<Task<bool>> send) =>
        new(user, "Email", user.Email, subject, subject, send);

    private static Outgoing Sms(User user, string logMessage, Func<Task<bool>> send) =>
        new(user, "SMS", user.PhoneNumber, null, logMessage, send);

    private static string FullName(User user) => $"{user.FirstName} {user.LastName}".Trim();

    private sealed record Outgoing(
        User User,
        string Type,
        string? Recipient,
        string? Subject,
        string LogMessage,
        Func<Task<bool>> Send);
}
