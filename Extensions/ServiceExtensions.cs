using Ride_Hailing_API.Services.Implementations;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPassengerService, PassengerService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IRideService, RideService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}