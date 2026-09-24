using Ride_Hailing_API.Domain.Settings;
using Ride_Hailing_API.Services.Implementations;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpMail>(configuration.GetSection("SmtpMail"));
        services.Configure<TelecomAbode>(configuration.GetSection("SmsSettings:TelecomAbode"));

        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpClient<ISmsService, TelecomAbodeSmsService>(client =>
            client.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPassengerService, PassengerService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IRideService, RideService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}