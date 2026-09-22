namespace Ride_Hailing_API.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string? Action { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}