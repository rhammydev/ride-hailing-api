using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class AuditLogRepository(AppDbContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log) =>
        await context.AuditLogs.AddAsync(log);

    public async Task<List<AuditLog>> GetAllAsync() =>
        await context.AuditLogs
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByUserIdAsync(int userId) =>
        await context.AuditLogs
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
