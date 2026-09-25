namespace Ride_Hailing_API.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .Select(NormalizeOrigin)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options => options.AddPolicy(FrontendPolicy, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT")
            .WithHeaders("Authorization", "Content-Type", "Accept")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

        return services;
    }

    private static string NormalizeOrigin(string origin)
    {
        if (origin.Trim() == "*" ||
            !Uri.TryCreate(origin.Trim(), UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") ||
            uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query))
        {
            throw new InvalidOperationException(
                $"Invalid CORS origin '{origin}'. Use scheme://host[:port], for example https://app.example.com. Wildcards are not allowed.");
        }

        return uri.GetLeftPart(UriPartial.Authority);
    }
}
