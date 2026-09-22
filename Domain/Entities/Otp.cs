using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Domain.Entities;

public class Otp
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string? Code { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}