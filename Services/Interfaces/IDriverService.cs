using Ride_Hailing_API.DTOs.Driver;
using Ride_Hailing_API.DTOs.Generic;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IDriverService
{
    Task<ApiResponse> SubmitOnboardingAsync(int driverId, DriverOnboardingRequest request);
    Task<ApiResponse> GetProfileAsync(int driverId);
    Task<ApiResponse> SetAvailabilityAsync(int driverId, DriverAvailabilityRequest request);
    Task<ApiResponse> GetAvailableRidesAsync(int driverId);
}
