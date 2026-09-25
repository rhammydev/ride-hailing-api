using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.DTOs.Admin;

public class DriverApplicationResponse
{
    public int? KycId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? DriverLicence { get; set; }
    public string? Nin { get; set; }
    public ApprovalStatus? Status { get; set; } // null when the driver has not submitted onboarding yet
    public bool IsAvailable { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public string? VehicleColor { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
