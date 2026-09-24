namespace Ride_Hailing_API.DTOs.Driver;

public class DriverOnboardingRequest
{
    public string DriverLicence { get; set; } = string.Empty;
    public string Nin { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehicleYear { get; set; } = string.Empty;
    public string VehicleColor { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int? Capacity { get; set; }
}
