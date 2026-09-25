using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.DTOs.Auth;

public class ResendOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
}
