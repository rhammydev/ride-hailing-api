using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Ride;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IRideService
{
    Task<ApiResponse> CreateRideAsync(int passengerId, CreateRideRequest request);
    Task<ApiResponse> GetPassengerRidesAsync(int passengerId);
    Task<ApiResponse> GetCurrentRideAsync(int passengerId);
    Task<ApiResponse> GetDriverRidesAsync(int driverId);
    Task<ApiResponse> GetRideByIdAsync(int rideId, int userId, UserRole role);
    Task<ApiResponse> AcceptRideAsync(int driverId, int rideId);
    Task<ApiResponse> UpdateRideStatusAsync(int driverId, int rideId, UpdateRideStatusRequest request);
    Task<ApiResponse> CancelRideAsync(int userId, UserRole role, int rideId, CancelRideRequest request);
}