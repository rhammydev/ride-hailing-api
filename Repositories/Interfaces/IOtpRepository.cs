using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface IOtpRepository
{
    Task<Otp?> GetLatestUnusedOtpAsync(int userId, OtpPurpose purpose);
    Task InvalidateUnusedOtpsAsync(int userId, OtpPurpose purpose);
    Task AddAsync(Otp otp);
    Task SaveChangesAsync();
}
