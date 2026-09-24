namespace Ride_Hailing_API.Services.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string? recipientNumber, string message);
}
