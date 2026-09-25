using Ride_Hailing_API.DTOs.Admin;
using Ride_Hailing_API.DTOs.Generic;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IAdminService
{
    Task<ApiResponse> GetAllUsersAsync();
    Task<ApiResponse> GetAllDriversAsync();
    Task<ApiResponse> GetPendingDriversAsync();
    Task<ApiResponse> ApproveDriverAsync(int adminId, int driverId);
    Task<ApiResponse> RejectDriverAsync(int adminId, int driverId, RejectDriverRequest request);
    Task<ApiResponse> UpdateUserStatusAsync(int adminId, int targetUserId, UpdateUserStatusRequest request);
    Task<ApiResponse> GetAllRidesAsync();
    Task<ApiResponse> GetAuditLogsAsync();
    Task<ApiResponse> GetNotificationsAsync();
}
