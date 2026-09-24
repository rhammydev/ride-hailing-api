using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phoneNumber);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<List<User>> GetAllUsersAsync();
    Task<List<User>> GetUsersByRoleAsync(UserRole role);
    Task AddAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
}
