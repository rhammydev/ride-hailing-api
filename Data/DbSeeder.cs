using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Data;

public class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Registration only allows Passenger/Driver, so the first Admin must be seeded.
        if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin)) return;

        var admin = new User
        {
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@ridehail.ng",
            PhoneNumber = "08000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@1"),
            Role = UserRole.Admin,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
