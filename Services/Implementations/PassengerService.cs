using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Passenger;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class PassengerService(
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    ILogger<PassengerService> logger) : IPassengerService
{
    public async Task<ApiResponse> GetProfileAsync(int passengerId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(passengerId);
            if (user == null)
            {
                return ApiResponse.Fail("Passenger profile not found.", 404, ResponseCodes.NotFound);
            }

            var profile = new PassengerProfileResponse
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return ApiResponse.Success("Profile retrieved successfully.", profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve profile for Passenger {PassengerId}", passengerId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving profile.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> UpdateProfileAsync(int passengerId, UpdateProfileRequest request)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(passengerId);
            if (user == null)
            {
                return ApiResponse.Fail("Passenger profile not found.", 404, ResponseCodes.NotFound);
            }

            var nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.Length > 0 ? nameParts[0] : request.FullName.Trim();
            user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var newPhone = request.PhoneNumber.Trim();
            if (user.PhoneNumber != newPhone)
            {
                if (await userRepository.PhoneExistsAsync(newPhone))
                {
                    return ApiResponse.Fail("Another user is already registered with this phone number.", 409, ResponseCodes.BadRequest);
                }
                user.PhoneNumber = newPhone;
                user.IsPhoneVerified = false;
            }

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "UpdateProfile",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var updatedProfile = new PassengerProfileResponse
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return ApiResponse.Success("Profile updated successfully.", updatedProfile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update profile for Passenger {PassengerId}", passengerId);
            return ApiResponse.Fail("An unexpected error occurred while updating profile.", 500, ResponseCodes.ServerError);
        }
    }
}
