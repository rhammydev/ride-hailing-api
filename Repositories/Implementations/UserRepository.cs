using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email) =>
        context.Users.AnyAsync(x => x.Email == email);

    public Task<bool> PhoneExistsAsync(string phoneNumber) =>
        context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);

    public Task<User?> GetByIdAsync(int id) =>
        context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> GetByPhoneNumberAsync(string phoneNumber) =>
        context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

    public async Task<List<User>> GetAllUsersAsync() =>
        await context.Users.OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role) =>
        await context.Users.Where(x => x.Role == role).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task AddAsync(User user) =>
        await context.Users.AddAsync(user);

    public void Update(User user) =>
        context.Users.Update(user);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
