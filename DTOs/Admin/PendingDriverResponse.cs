using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.DTOs.Admin;

public class PendingDriverResponse
{
    public int KycId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DriverLicence { get; set; } = string.Empty;
    public string Nin { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehiclePlateNumber { get; set; }
    public string? VehicleColor { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
