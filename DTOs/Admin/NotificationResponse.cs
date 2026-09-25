namespace Ride_Hailing_API.DTOs.Admin;

public class NotificationResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
