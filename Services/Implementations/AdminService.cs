using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.DTOs.Admin;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Ride;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class AdminService(
    IUserRepository userRepository,
    IDriverRepository driverRepository,
    IRideRepository rideRepository,
    IAuditLogRepository auditLogRepository,
    INotificationService notificationService,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<ApiResponse> GetAllUsersAsync()
    {
        try
        {
            var users = await userRepository.GetAllUsersAsync();
            var response = users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Role = u.Role,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                IsPhoneVerified = u.IsPhoneVerified,
                CreatedAt = u.CreatedAt
            }).ToList();

            return ApiResponse.Success("Users retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve all users for Admin");
            return ApiResponse.Fail("An unexpected error occurred while retrieving users.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetPendingDriversAsync()
    {
        try
        {
            var pendingKycs = await driverRepository.GetPendingDriversAsync();
            var result = new List<PendingDriverResponse>();

            foreach (var kyc in pendingKycs)
            {
                var vehicle = await driverRepository.GetVehicleByDriverIdAsync(kyc.UserId);
                result.Add(new PendingDriverResponse
                {
                    KycId = kyc.Id,
                    UserId = kyc.UserId,
                    FullName = kyc.User != null ? $"{kyc.User.FirstName} {kyc.User.LastName}".Trim() : string.Empty,
                    Email = kyc.User?.Email ?? string.Empty,
                    PhoneNumber = kyc.User?.PhoneNumber ?? string.Empty,
                    DriverLicence = kyc.DriverLicence ?? string.Empty,
                    Nin = kyc.Nin ?? string.Empty,
                    Status = kyc.Status,
                    VehicleMake = vehicle?.Make,
                    VehicleModel = vehicle?.Model,
                    VehiclePlateNumber = vehicle?.PlateNumber,
                    VehicleColor = vehicle?.Color,
                    SubmittedAt = kyc.ApprovedAt
                });
            }

            return ApiResponse.Success("Pending driver applications retrieved successfully.", result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve pending drivers for Admin");
            return ApiResponse.Fail("An unexpected error occurred while retrieving pending drivers.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> ApproveDriverAsync(int adminId, int driverId)
    {
        try
        {
            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null)
            {
                return ApiResponse.Fail("Driver application not found.", 404, ResponseCodes.NotFound);
            }

            kyc.Status = ApprovalStatus.Approved;
            kyc.ApproverId = adminId;
            kyc.ApprovedAt = DateTime.UtcNow;

            driverRepository.UpdateKyc(kyc);
            await driverRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = adminId,
                Action = "DriverApproval",
                Status = "Approved",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var driverUser = await userRepository.GetByIdAsync(driverId);
            if (driverUser != null)
            {
                await notificationService.SendDriverApprovedAsync(driverUser);
            }

            logger.LogInformation("Driver {DriverId} approved by Admin {AdminId}", driverId, adminId);

            return ApiResponse.Success("Driver application approved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to approve Driver {DriverId} by Admin {AdminId}", driverId, adminId);
            return ApiResponse.Fail("An unexpected error occurred while approving driver application.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> RejectDriverAsync(int adminId, int driverId, RejectDriverRequest request)
    {
        try
        {
            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null)
            {
                return ApiResponse.Fail("Driver application not found.", 404, ResponseCodes.NotFound);
            }

            kyc.Status = ApprovalStatus.Rejected;
            kyc.IsAvailable = false;

            driverRepository.UpdateKyc(kyc);
            await driverRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = adminId,
                Action = "DriverRejection",
                Status = "Rejected",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var driverUser = await userRepository.GetByIdAsync(driverId);
            if (driverUser != null)
            {
                await notificationService.SendDriverRejectedAsync(driverUser, request.Reason);
            }

            logger.LogInformation("Driver {DriverId} rejected by Admin {AdminId}", driverId, adminId);

            return ApiResponse.Success("Driver application rejected successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reject Driver {DriverId} by Admin {AdminId}", driverId, adminId);
            return ApiResponse.Fail("An unexpected error occurred while rejecting driver application.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> UpdateUserStatusAsync(int adminId, int targetUserId, UpdateUserStatusRequest request)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(targetUserId);
            if (user == null)
            {
                return ApiResponse.Fail("User not found.", 404, ResponseCodes.NotFound);
            }

            user.IsActive = request.IsActive;
            userRepository.Update(user);

            if (!request.IsActive && user.Role == UserRole.Driver)
            {
                var kyc = await driverRepository.GetKycByUserIdAsync(targetUserId);
                if (kyc != null)
                {
                    kyc.IsAvailable = false;
                    driverRepository.UpdateKyc(kyc);
                }
            }

            await userRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = adminId,
                Action = "UserStatusUpdate",
                Status = request.IsActive ? "Activated" : "Deactivated",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var actionWord = request.IsActive ? "activated" : "deactivated";
            logger.LogInformation("User {TargetUserId} {ActionWord} by Admin {AdminId}", targetUserId, actionWord, adminId);

            return ApiResponse.Success($"User has been {actionWord} successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update status for User {TargetUserId} by Admin {AdminId}", targetUserId, adminId);
            return ApiResponse.Fail("An unexpected error occurred while updating user status.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetAllRidesAsync()
    {
        try
        {
            var rides = await rideRepository.GetAllRidesAsync();
            var response = rides.Select(r => new RideResponse
            {
                Id = r.Id,
                Reference = r.Reference ?? string.Empty,
                PassengerId = r.PassengerId,
                PassengerName = r.Passenger != null ? $"{r.Passenger.FirstName} {r.Passenger.LastName}".Trim() : string.Empty,
                DriverId = r.DriverId,
                DriverName = r.Driver != null ? $"{r.Driver.FirstName} {r.Driver.LastName}".Trim() : null,
                PickupLocation = r.PickupLocation ?? string.Empty,
                Destination = r.Destination ?? string.Empty,
                Status = r.Status,
                CancellationReason = r.CancellationReason,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt,
                CancelledAt = r.CancelledAt
            }).ToList();

            return ApiResponse.Success("All rides retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve all rides for Admin");
            return ApiResponse.Fail("An unexpected error occurred while retrieving rides.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetAuditLogsAsync()
    {
        try
        {
            var logs = await auditLogRepository.GetAllAsync();
            var response = logs.Select(l => new AuditLogResponse
            {
                Id = l.Id,
                UserId = l.UserId,
                UserEmail = l.User?.Email,
                Action = l.Action ?? string.Empty,
                Status = l.Status ?? string.Empty,
                CreatedAt = l.CreatedAt
            }).ToList();

            return ApiResponse.Success("Audit logs retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve audit logs for Admin");
            return ApiResponse.Fail("An unexpected error occurred while retrieving audit logs.", 500, ResponseCodes.ServerError);
        }
    }
}
