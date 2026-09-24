using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    public async Task AddAsync(Notification notification) =>
        await context.Notifications.AddAsync(notification);

    public async Task<List<Notification>> GetByUserIdAsync(int userId) =>
        await context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<List<Notification>> GetPendingAsync() =>
        await context.Notifications
            .Where(x => !x.IsSent)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public void Update(Notification notification) =>
        context.Notifications.Update(notification);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
