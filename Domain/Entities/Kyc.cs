using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Domain.Entities;

public class Kyc
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string? DriverLicence { get; set; }
    public string? Nin { get; set; }
    public ApprovalStatus Status { get; set; }
    public bool IsAvailable { get; set; }
    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}