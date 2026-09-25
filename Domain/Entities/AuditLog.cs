namespace Ride_Hailing_API.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string? Action { get; set; }
    public string? Status { get; set; }
    public string? TargetEntity { get; set; }
    public int? TargetId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}