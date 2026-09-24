using Ride_Hailing_API.DTOs.Auth;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Passenger;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse> GetAllUsersAsync();
    Task<ApiResponse> GetUserByIdAsync(int userId);
    Task<ApiResponse> CreateUserAsync(RegisterRequest request);
    Task<ApiResponse> UpdateUserAsync(int userId, UpdateProfileRequest request);
}