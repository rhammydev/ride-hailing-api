namespace Ride_Hailing_API.Services.Interfaces;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string? recipientNumber, string message);
}
