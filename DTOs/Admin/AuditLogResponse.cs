namespace Ride_Hailing_API.DTOs.Admin;

public class AuditLogResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TargetEntity { get; set; }
    public int? TargetId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
