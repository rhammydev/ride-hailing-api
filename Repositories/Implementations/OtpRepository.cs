using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class OtpRepository(AppDbContext context) : IOtpRepository
{
    public Task<Otp?> GetLatestUnusedOtpAsync(int userId, OtpPurpose purpose) =>
        context.Otps
            .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task InvalidateUnusedOtpsAsync(int userId, OtpPurpose purpose)
    {
        var otps = await context.Otps
            .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
        {
            otp.IsUsed = true;
        }
    }

    public async Task AddAsync(Otp otp) =>
        await context.Otps.AddAsync(otp);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
