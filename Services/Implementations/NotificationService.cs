using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Services.Interfaces;
using Ride_Hailing_API.Utilities;

namespace Ride_Hailing_API.Services.Implementations;

public class NotificationService(IEmailService email, ISmsService sms) : INotificationService
{
    public Task SendOtpEmailAsync(string emailAddress, string fullName, string otp) =>
        email.SendOtpAsync(emailAddress, fullName, otp);

    public Task SendOtpSmsAsync(string? recipient, string otp) =>
        sms.SendSmsAsync(recipient, SmsUtils.Otp(otp));

    public Task SendWelcomeEmailAsync(string emailAddress, string fullName, string userId) =>
        email.SendWelcomeAsync(emailAddress, fullName, userId);

    public Task SendAccountVerifiedEmailAsync(string emailAddress, string fullName, string userId) =>
        email.SendAccountVerifiedAsync(emailAddress, fullName, userId);

    public Task SendLoginAlertAsync(string emailAddress, string fullName) =>
        email.SendLoginAlertAsync(emailAddress, fullName);

    public Task SendPasswordChangedAsync(string emailAddress, string fullName, string? phoneNumber) =>
        Task.WhenAll(
            email.SendPasswordChangedAsync(emailAddress, fullName),
            sms.SendSmsAsync(phoneNumber, SmsUtils.PasswordChanged()));

    public Task SendPasswordResetOtpAsync(
        string emailAddress,
        string fullName,
        string? phoneNumber,
        string otp) =>
        Task.WhenAll(
            email.SendPasswordResetOtpAsync(emailAddress, fullName, otp),
            sms.SendSmsAsync(phoneNumber, SmsUtils.PasswordResetOtp(otp)));

    public Task SendProfileUpdatedEmailAsync(string emailAddress, string fullName) =>
        email.SendProfileUpdatedAsync(emailAddress, fullName);

    public Task SendAccountDeactivatedAsync(string emailAddress, string fullName, string? phoneNumber) =>
        Task.WhenAll(
            email.SendAccountDeactivatedAsync(emailAddress, fullName),
            sms.SendSmsAsync(phoneNumber, SmsUtils.AccountDeactivated()));

    public Task SendRideStatusAsync(User passenger, User? driver, Ride ride, string status)
    {
        var rideRef = ride.Reference ?? ride.Id.ToString();
        var pickup = ride.PickupLocation ?? "N/A";
        var destination = ride.Destination ?? "N/A";
        var subject = $"Ride {status} - {rideRef}";

        var tasks = new List<Task>
        {
            email.SendRideStatusAsync(passenger.Email!, FullName(passenger), subject, rideRef,
                pickup, destination, status, ride.CancellationReason),
            sms.SendSmsAsync(passenger.PhoneNumber, SmsUtils.RideStatus(rideRef, status))
        };

        if (driver != null)
        {
            tasks.Add(email.SendRideStatusAsync(driver.Email!, FullName(driver), subject, rideRef,
                pickup, destination, status, ride.CancellationReason));
            tasks.Add(sms.SendSmsAsync(driver.PhoneNumber, SmsUtils.RideStatus(rideRef, status)));
        }

        return Task.WhenAll(tasks);
    }

    public Task SendDriverApprovedAsync(User driver) =>
        Task.WhenAll(
            email.SendDriverApprovedAsync(driver.Email!, FullName(driver)),
            sms.SendSmsAsync(driver.PhoneNumber, SmsUtils.DriverApproved()));

    public Task SendDriverRejectedAsync(User driver, string? reason) =>
        Task.WhenAll(
            email.SendDriverRejectedAsync(driver.Email!, FullName(driver), reason),
            sms.SendSmsAsync(driver.PhoneNumber, SmsUtils.DriverRejected(reason)));

    private static string FullName(User user) => $"{user.FirstName} {user.LastName}";
}
