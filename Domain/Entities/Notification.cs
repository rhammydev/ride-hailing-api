namespace Ride_Hailing_API.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Type { get; set; } = string.Empty; // "Email" or "SMS"
    public string Recipient { get; set; } = string.Empty; // email address or phone number
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
