using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Passenger;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IPassengerService
{
    Task<ApiResponse> GetProfileAsync(int passengerId);
    Task<ApiResponse> UpdateProfileAsync(int passengerId, UpdateProfileRequest request);
}
