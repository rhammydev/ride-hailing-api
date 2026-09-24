using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class NotificationService(
    INotificationRepository notificationRepository,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<ApiResponse> SendEmailNotificationAsync(int userId, string recipientEmail, string subject, string message)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = "Email",
                Recipient = recipientEmail,
                Subject = subject,
                Message = message,
                IsSent = true,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await notificationRepository.AddAsync(notification);
            await notificationRepository.SaveChangesAsync();

            logger.LogInformation("Email notification sent to User {UserId} ({Recipient}): {Subject}", userId, recipientEmail, subject);

            return ApiResponse.Success("Email notification sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email notification to User {UserId} ({Recipient})", userId, recipientEmail);
            return ApiResponse.Fail("Failed to send email notification.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> SendSmsNotificationAsync(int userId, string recipientPhone, string message)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = "SMS",
                Recipient = recipientPhone,
                Subject = "SMS Alert",
                Message = message,
                IsSent = true,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await notificationRepository.AddAsync(notification);
            await notificationRepository.SaveChangesAsync();

            logger.LogInformation("SMS notification sent to User {UserId} ({Recipient}): {Message}", userId, recipientPhone, message);

            return ApiResponse.Success("SMS notification sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS notification to User {UserId} ({Recipient})", userId, recipientPhone);
            return ApiResponse.Fail("Failed to send SMS notification.", 500, ResponseCodes.ServerError);
        }
    }
}
