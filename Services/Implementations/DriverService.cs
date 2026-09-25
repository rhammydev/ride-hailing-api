using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.DTOs.Driver;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Ride;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class DriverService(
    IUserRepository userRepository,
    IDriverRepository driverRepository,
    IRideRepository rideRepository,
    IAuditLogRepository auditLogRepository,
    ILogger<DriverService> logger) : IDriverService
{
    public async Task<ApiResponse> SubmitOnboardingAsync(int driverId, DriverOnboardingRequest request)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(driverId);
            if (user == null || user.Role != UserRole.Driver)
            {
                return ApiResponse.Fail("Driver account not found.", 404, ResponseCodes.NotFound);
            }

            var plate = request.PlateNumber.Trim().ToUpperInvariant();
            var existingVehicleWithPlate = await driverRepository.GetVehicleByDriverIdAsync(driverId);
            if (existingVehicleWithPlate == null || existingVehicleWithPlate.PlateNumber != plate)
            {
                if (await driverRepository.PlateNumberExistsAsync(plate))
                {
                    return ApiResponse.Fail("A vehicle with this license plate is already registered.", 409, ResponseCodes.BadRequest);
                }
            }

            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null)
            {
                kyc = new Kyc
                {
                    UserId = driverId,
                    DriverLicence = request.DriverLicence.Trim(),
                    Nin = request.Nin.Trim(),
                    Status = ApprovalStatus.Pending,
                    IsAvailable = false,
                    SubmittedAt = DateTime.UtcNow
                };
                await driverRepository.AddKycAsync(kyc);
            }
            else
            {
                kyc.DriverLicence = request.DriverLicence.Trim();
                kyc.Nin = request.Nin.Trim();
                // Resubmitting sends the application back for review.
                kyc.Status = ApprovalStatus.Pending;
                kyc.IsAvailable = false;
                kyc.ApproverId = null;
                kyc.ApprovedAt = null;
                kyc.SubmittedAt = DateTime.UtcNow;
                driverRepository.UpdateKyc(kyc);
            }

            var vehicle = await driverRepository.GetVehicleByDriverIdAsync(driverId);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    DriverId = driverId,
                    Make = request.VehicleMake.Trim(),
                    Model = request.VehicleModel.Trim(),
                    Year = request.VehicleYear.Trim(),
                    Color = request.VehicleColor.Trim(),
                    PlateNumber = plate,
                    Capacity = request.Capacity
                };
                await driverRepository.AddVehicleAsync(vehicle);
            }
            else
            {
                vehicle.Make = request.VehicleMake.Trim();
                vehicle.Model = request.VehicleModel.Trim();
                vehicle.Year = request.VehicleYear.Trim();
                vehicle.Color = request.VehicleColor.Trim();
                vehicle.PlateNumber = plate;
                vehicle.Capacity = request.Capacity;
                driverRepository.UpdateVehicle(vehicle);
            }

            await driverRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = driverId,
                Action = "DriverOnboarding",
                Status = "Success",
                TargetEntity = "Driver",
                TargetId = driverId,
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            logger.LogInformation("Driver {DriverId} submitted onboarding application.", driverId);

            return ApiResponse.Success("Driver and vehicle information submitted successfully. Pending Admin approval.", statusCode: 201);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Driver onboarding failed for Driver {DriverId}", driverId);
            return ApiResponse.Fail("An unexpected error occurred during onboarding submission.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetProfileAsync(int driverId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(driverId);
            if (user == null || user.Role != UserRole.Driver)
            {
                return ApiResponse.Fail("Driver not found.", 404, ResponseCodes.NotFound);
            }

            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            var vehicle = await driverRepository.GetVehicleByDriverIdAsync(driverId);

            var profile = new DriverProfileResponse
            {
                Id = kyc?.Id ?? 0,
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                DriverLicence = kyc?.DriverLicence ?? string.Empty,
                Nin = kyc?.Nin ?? string.Empty,
                Status = kyc?.Status ?? ApprovalStatus.Pending,
                IsAvailable = kyc?.IsAvailable ?? false,
                VehicleMake = vehicle?.Make,
                VehicleModel = vehicle?.Model,
                VehiclePlateNumber = vehicle?.PlateNumber,
                VehicleColor = vehicle?.Color
            };

            return ApiResponse.Success("Driver profile retrieved successfully.", profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve profile for Driver {DriverId}", driverId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving driver profile.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> SetAvailabilityAsync(int driverId, DriverAvailabilityRequest request)
    {
        try
        {
            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null)
            {
                return ApiResponse.Fail("Driver profile not found. Please complete onboarding first.", 404, ResponseCodes.NotFound);
            }

            if (kyc.Status != ApprovalStatus.Approved)
            {
                return ApiResponse.Fail("Your account must be approved by an Admin before you can change your availability.", 403, ResponseCodes.Forbidden);
            }

            kyc.IsAvailable = request.IsAvailable;
            driverRepository.UpdateKyc(kyc);
            await driverRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = driverId,
                Action = "SetAvailability",
                Status = request.IsAvailable ? "Available" : "Unavailable",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var statusMessage = request.IsAvailable ? "Driver marked as Available." : "Driver marked as Unavailable.";
            return ApiResponse.Success(statusMessage, new { isAvailable = kyc.IsAvailable });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update availability for Driver {DriverId}", driverId);
            return ApiResponse.Fail("An unexpected error occurred while updating availability.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetAvailableRidesAsync(int driverId)
    {
        try
        {
            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null || kyc.Status != ApprovalStatus.Approved)
            {
                return ApiResponse.Fail("Only approved drivers can view available ride requests.", 403, ResponseCodes.Forbidden);
            }

            if (!kyc.IsAvailable)
            {
                return ApiResponse.Fail("You must set your status to Available to view available ride requests.", 403, ResponseCodes.Forbidden);
            }

            var rides = await rideRepository.GetAvailableRidesAsync();
            var rideResponses = rides.Select(r => new RideResponse
            {
                Id = r.Id,
                Reference = r.Reference ?? string.Empty,
                PassengerId = r.PassengerId,
                PassengerName = r.Passenger != null ? $"{r.Passenger.FirstName} {r.Passenger.LastName}".Trim() : string.Empty,
                DriverId = r.DriverId,
                DriverName = null,
                PickupLocation = r.PickupLocation ?? string.Empty,
                Destination = r.Destination ?? string.Empty,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }).ToList();

            return ApiResponse.Success("Available rides retrieved successfully.", rideResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve available rides for Driver {DriverId}", driverId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving available rides.", 500, ResponseCodes.ServerError);
        }
    }
}
