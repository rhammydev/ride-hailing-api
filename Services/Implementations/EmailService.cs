using Ride_Hailing_API.Domain.Settings;
using Ride_Hailing_API.Services.Interfaces;
using Ride_Hailing_API.Utilities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Ride_Hailing_API.Services.Implementations;

public class EmailService(IOptions<SmtpMail> smtpMail, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpMail _smtpMail = smtpMail.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<bool> SendOtpAsync(string toEmail, string fullName, string otp)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your RideHail verification code");
        message.Body = CreateHtmlBody(MailUtils.Otp(fullName, otp));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendWelcomeAsync(string toEmail, string fullName, string userId)
    {
        var message = CreateBaseMessage(toEmail, fullName, $"Welcome to RideHail, {fullName}");
        message.Body = CreateHtmlBody(MailUtils.Welcome(fullName, toEmail, userId));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendAccountVerifiedAsync(string toEmail, string fullName, string userId)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your RideHail account is active");
        message.Body = CreateHtmlBody(MailUtils.AccountVerified(fullName, toEmail, userId));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendLoginAlertAsync(string toEmail, string fullName)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Security alert: new account login");
        message.Body = CreateHtmlBody(MailUtils.LoginAlert(fullName, DateTime.UtcNow));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendPasswordChangedAsync(string toEmail, string fullName)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your RideHail account password was changed");
        message.Body = CreateHtmlBody(MailUtils.PasswordChanged(fullName, DateTime.UtcNow));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendPasswordResetOtpAsync(string toEmail, string fullName, string otp)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Reset your RideHail password");
        message.Body = CreateHtmlBody(MailUtils.PasswordResetOtp(fullName, otp));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendProfileUpdatedAsync(string toEmail, string fullName)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your RideHail profile was updated");
        message.Body = CreateHtmlBody(MailUtils.ProfileUpdated(fullName, DateTime.UtcNow));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendAccountDeactivatedAsync(string toEmail, string fullName)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your RideHail account was deactivated");
        message.Body = CreateHtmlBody(MailUtils.AccountDeactivated(fullName, DateTime.UtcNow));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendRideStatusAsync(
        string toEmail,
        string fullName,
        string subject,
        string rideReference,
        string pickupLocation,
        string destination,
        string status,
        string? cancellationReason)
    {
        var message = CreateBaseMessage(toEmail, fullName, subject);
        message.Body = CreateHtmlBody(
            MailUtils.RideStatus(fullName, rideReference, pickupLocation, destination, status, cancellationReason));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendDriverApprovedAsync(string toEmail, string fullName)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Your driver application has been approved");
        message.Body = CreateHtmlBody(MailUtils.DriverApproved(fullName));

        return await SendMimeMessageAsync(message);
    }

    public async Task<bool> SendDriverRejectedAsync(string toEmail, string fullName, string? reason)
    {
        var message = CreateBaseMessage(toEmail, fullName, "Driver application update");
        message.Body = CreateHtmlBody(MailUtils.DriverRejected(fullName, reason));

        return await SendMimeMessageAsync(message);
    }

    private MimeMessage CreateBaseMessage(string toEmail, string toName, string subject)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpMail.SenderName, _smtpMail.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        return message;
    }

    private static TextPart CreateHtmlBody(string html) => new(TextFormat.Html)
    {
        Text = html
    };

    private async Task<bool> SendMimeMessageAsync(MimeMessage message)
    {
        if (string.IsNullOrWhiteSpace(_smtpMail.Server) ||
            string.IsNullOrWhiteSpace(_smtpMail.Username) ||
            string.IsNullOrWhiteSpace(_smtpMail.Password))
        {
            _logger.LogInformation("SMTP settings are incomplete. Skipping email to {Recipient}.", message.To);
            return false;
        }

        using var client = new SmtpClient
        {
            CheckCertificateRevocation = false
        };
        try
        {
            await client.ConnectAsync(_smtpMail.Server, _smtpMail.Port, false);
            await client.AuthenticateAsync(_smtpMail.Username, _smtpMail.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}.", message.To);
            return false;
        }
    }
}
