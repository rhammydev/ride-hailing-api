using Ride_Hailing_API.DTOs.Generic;

namespace Ride_Hailing_API.Services.Interfaces;

public interface INotificationService
{
    Task<ApiResponse> SendEmailNotificationAsync(int userId, string recipientEmail, string subject, string message);
    Task<ApiResponse> SendSmsNotificationAsync(int userId, string recipientPhone, string message);
}
