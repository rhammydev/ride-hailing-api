using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ride_Hailing_API.Domain.Settings;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var jwtSettings = section.Get<JwtSettings>() ?? new JwtSettings();

        if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32 ||
            string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
            string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            throw new InvalidOperationException(
                "JWT is not configured. Set Jwt:Key (at least 32 bytes), Jwt:Issuer and Jwt:Audience.");
        }

        services.Configure<JwtSettings>(section);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Return the standard ApiResponse body for 401 and 403 instead of an empty response.
                options.Events = new JwtBearerEvents
                {
                    // Tokens issued before a user was deactivated must stop working immediately.
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!int.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("Token does not identify a user.");
                            return;
                        }

                        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var user = await users.GetByIdAsync(userId);
                        if (user is not { IsActive: true })
                        {
                            context.Fail("User account is inactive or no longer exists.");
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                            "Authentication is required. Provide a valid bearer token.",
                            401,
                            ResponseCodes.Unauthorized));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                            "You do not have permission to access this resource.",
                            403,
                            ResponseCodes.Forbidden));
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
