using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<List<AuditLog>> GetAllAsync();
    Task<List<AuditLog>> GetByUserIdAsync(int userId);
    Task SaveChangesAsync();
}
