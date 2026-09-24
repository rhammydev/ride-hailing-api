namespace Ride_Hailing_API.DTOs.Auth;

public class VerifyPhoneRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
