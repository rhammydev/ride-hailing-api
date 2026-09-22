using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Domain.Entities;

public class User
{
    public int  Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}