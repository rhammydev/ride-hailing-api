using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<List<Notification>> GetPendingAsync();
    void Update(Notification notification);
    Task SaveChangesAsync();
}
