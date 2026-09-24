using Ride_Hailing_API.Repositories.Implementations;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IRideRepository, RideRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
