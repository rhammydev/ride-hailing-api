namespace Ride_Hailing_API.DTOs.Auth;

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
