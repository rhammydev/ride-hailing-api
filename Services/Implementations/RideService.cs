using System.Security.Cryptography;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Ride;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class RideService(
    IRideRepository rideRepository,
    IDriverRepository driverRepository,
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    INotificationService notificationService,
    ILogger<RideService> logger) : IRideService
{
    public async Task<ApiResponse> CreateRideAsync(int passengerId, CreateRideRequest request)
    {
        try
        {
            var passenger = await userRepository.GetByIdAsync(passengerId);
            if (passenger == null || passenger.Role != UserRole.Passenger)
            {
                return ApiResponse.Fail("Passenger account not found.", 404, ResponseCodes.NotFound);
            }

            var activeRide = await rideRepository.GetActiveRideForPassengerAsync(passengerId);
            if (activeRide != null)
            {
                return ApiResponse.Fail("You already have an active ride request.", 400, ResponseCodes.BadRequest);
            }

            var reference = await GenerateUniqueRideReferenceAsync();

            var ride = new Ride
            {
                PassengerId = passengerId,
                DriverId = null,
                Reference = reference,
                PickupLocation = request.PickupLocation.Trim(),
                Destination = request.Destination.Trim(),
                Status = RideStatus.Requested,
                CreatedAt = DateTime.UtcNow
            };

            await rideRepository.AddAsync(ride);
            await rideRepository.SaveChangesAsync();

            await rideRepository.AddStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = RideStatus.Requested,
                NewStatus = RideStatus.Requested,
                ChangedBy = $"Passenger:{passengerId}",
                Reason = "Ride requested by passenger.",
                CreatedAt = DateTime.UtcNow
            });

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = passengerId,
                Action = "RideCreation",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });

            await rideRepository.SaveChangesAsync();
            await auditLogRepository.SaveChangesAsync();

            logger.LogInformation("Ride {Reference} created by Passenger {PassengerId}", ride.Reference, passengerId);

            var response = MapToResponse(ride, passenger, null);
            return ApiResponse.Success("Ride requested successfully.", response, 201);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create ride for Passenger {PassengerId}", passengerId);
            return ApiResponse.Fail("An unexpected error occurred while creating the ride.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetPassengerRidesAsync(int passengerId)
    {
        try
        {
            var rides = await rideRepository.GetPassengerRidesAsync(passengerId);
            var response = rides.Select(r => MapToResponse(r, r.Passenger, r.Driver)).ToList();
            return ApiResponse.Success("Passenger rides retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve rides for Passenger {PassengerId}", passengerId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving rides.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetDriverRidesAsync(int driverId)
    {
        try
        {
            var rides = await rideRepository.GetDriverRidesAsync(driverId);
            var response = rides.Select(r => MapToResponse(r, r.Passenger, r.Driver)).ToList();
            return ApiResponse.Success("Driver rides retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve rides for Driver {DriverId}", driverId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving rides.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> GetRideByIdAsync(int rideId, int userId, UserRole role)
    {
        try
        {
            var ride = await rideRepository.GetByIdAsync(rideId);
            if (ride == null)
            {
                return ApiResponse.Fail("Ride not found.", 404, ResponseCodes.NotFound);
            }

            // Story 43 & 51: Resource ownership check
            if (role == UserRole.Passenger && ride.PassengerId != userId)
            {
                return ApiResponse.Fail("Access denied. You do not own this ride.", 403, ResponseCodes.Forbidden);
            }

            if (role == UserRole.Driver && ride.DriverId != userId && ride.Status != RideStatus.Requested)
            {
                return ApiResponse.Fail("Access denied. You are not assigned to this ride.", 403, ResponseCodes.Forbidden);
            }

            var response = MapToResponse(ride, ride.Passenger, ride.Driver);
            return ApiResponse.Success("Ride retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve Ride {RideId} for User {UserId}", rideId, userId);
            return ApiResponse.Fail("An unexpected error occurred while retrieving the ride.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> AcceptRideAsync(int driverId, int rideId)
    {
        try
        {
            var kyc = await driverRepository.GetKycByUserIdAsync(driverId);
            if (kyc == null || kyc.Status != ApprovalStatus.Approved)
            {
                return ApiResponse.Fail("Only approved drivers can accept rides.", 403, ResponseCodes.Forbidden);
            }

            if (!kyc.IsAvailable)
            {
                return ApiResponse.Fail("You must set your status to Available before accepting rides.", 400, ResponseCodes.BadRequest);
            }

            var activeRide = await rideRepository.GetActiveRideForDriverAsync(driverId);
            if (activeRide != null)
            {
                return ApiResponse.Fail("You already have an active ride in progress.", 400, ResponseCodes.BadRequest);
            }

            var ride = await rideRepository.GetByIdAsync(rideId);
            if (ride == null)
            {
                return ApiResponse.Fail("Ride not found.", 404, ResponseCodes.NotFound);
            }

            if (ride.Status != RideStatus.Requested || ride.DriverId != null)
            {
                return ApiResponse.Fail("Ride is no longer available to accept.", 400, ResponseCodes.BadRequest);
            }

            var previousStatus = ride.Status;
            ride.DriverId = driverId;
            ride.Status = RideStatus.Accepted;

            rideRepository.Update(ride);

            await rideRepository.AddStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = previousStatus,
                NewStatus = RideStatus.Accepted,
                ChangedBy = $"Driver:{driverId}",
                Reason = "Ride accepted by driver.",
                CreatedAt = DateTime.UtcNow
            });

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = driverId,
                Action = "RideAcceptance",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });

            await rideRepository.SaveChangesAsync();
            await auditLogRepository.SaveChangesAsync();

            // Story 72: Notify passenger when driver accepts ride
            if (ride.Passenger != null && !string.IsNullOrEmpty(ride.Passenger.Email))
            {
                var driver = await userRepository.GetByIdAsync(driverId);
                var driverName = driver != null ? $"{driver.FirstName} {driver.LastName}".Trim() : "A driver";
                await notificationService.SendEmailNotificationAsync(
                    ride.PassengerId,
                    ride.Passenger.Email,
                    "Ride Accepted!",
                    $"{driverName} has accepted your ride request ({ride.Reference})."
                );
            }

            logger.LogInformation("Ride {RideId} accepted by Driver {DriverId}", rideId, driverId);

            var driverUser = await userRepository.GetByIdAsync(driverId);
            var response = MapToResponse(ride, ride.Passenger, driverUser);
            return ApiResponse.Success("Ride accepted successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to accept Ride {RideId} by Driver {DriverId}", rideId, driverId);
            return ApiResponse.Fail("An unexpected error occurred while accepting the ride.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> UpdateRideStatusAsync(int driverId, int rideId, UpdateRideStatusRequest request)
    {
        try
        {
            var ride = await rideRepository.GetByIdAsync(rideId);
            if (ride == null)
            {
                return ApiResponse.Fail("Ride not found.", 404, ResponseCodes.NotFound);
            }

            // Story 51: Driver cannot update ride not assigned to them
            if (ride.DriverId != driverId)
            {
                return ApiResponse.Fail("Access denied. You are not assigned to this ride.", 403, ResponseCodes.Forbidden);
            }

            if (!IsValidDriverTransition(ride.Status, request.Status))
            {
                return ApiResponse.Fail(
                    $"Invalid status transition from {ride.Status} to {request.Status}.",
                    400,
                    ResponseCodes.BadRequest
                );
            }

            var previousStatus = ride.Status;
            ride.Status = request.Status;

            if (request.Status == RideStatus.Completed)
            {
                ride.CompletedAt = DateTime.UtcNow;
            }

            rideRepository.Update(ride);

            await rideRepository.AddStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = previousStatus,
                NewStatus = request.Status,
                ChangedBy = $"Driver:{driverId}",
                Reason = $"Status updated to {request.Status} by driver.",
                CreatedAt = DateTime.UtcNow
            });

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = driverId,
                Action = "RideStatusChange",
                Status = request.Status.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await rideRepository.SaveChangesAsync();
            await auditLogRepository.SaveChangesAsync();

            // Story 73: Notify passenger when driver arrives
            if (request.Status == RideStatus.DriverArrived && ride.Passenger != null && !string.IsNullOrEmpty(ride.Passenger.Email))
            {
                await notificationService.SendEmailNotificationAsync(
                    ride.PassengerId,
                    ride.Passenger.Email,
                    "Driver Has Arrived!",
                    $"Your driver has arrived at {ride.PickupLocation} for ride {ride.Reference}."
                );
            }

            // Story 75: Notify passenger when ride completed
            if (request.Status == RideStatus.Completed && ride.Passenger != null && !string.IsNullOrEmpty(ride.Passenger.Email))
            {
                await notificationService.SendEmailNotificationAsync(
                    ride.PassengerId,
                    ride.Passenger.Email,
                    "Ride Completed",
                    $"Your ride ({ride.Reference}) to {ride.Destination} has been completed. Thank you for riding with us!"
                );
            }

            logger.LogInformation("Ride {RideId} transitioned to {NewStatus} by Driver {DriverId}", rideId, request.Status, driverId);

            var response = MapToResponse(ride, ride.Passenger, ride.Driver);
            return ApiResponse.Success($"Ride status updated to {request.Status}.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update status for Ride {RideId} by Driver {DriverId}", rideId, driverId);
            return ApiResponse.Fail("An unexpected error occurred while updating ride status.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> CancelRideAsync(int userId, UserRole role, int rideId, CancelRideRequest request)
    {
        try
        {
            var ride = await rideRepository.GetByIdAsync(rideId);
            if (ride == null)
            {
                return ApiResponse.Fail("Ride not found.", 404, ResponseCodes.NotFound);
            }

            // Ownership checks
            if (role == UserRole.Passenger && ride.PassengerId != userId)
            {
                return ApiResponse.Fail("Access denied. You do not own this ride.", 403, ResponseCodes.Forbidden);
            }
            if (role == UserRole.Driver && ride.DriverId != userId)
            {
                return ApiResponse.Fail("Access denied. You are not assigned to this ride.", 403, ResponseCodes.Forbidden);
            }

            // Story 67: Completed rides cannot be cancelled; InProgress rides cannot be cancelled directly
            if (ride.Status == RideStatus.Completed)
            {
                return ApiResponse.Fail("Completed rides cannot be cancelled.", 400, ResponseCodes.BadRequest);
            }
            if (ride.Status == RideStatus.Cancelled)
            {
                return ApiResponse.Fail("Ride is already cancelled.", 400, ResponseCodes.BadRequest);
            }
            if (ride.Status == RideStatus.InProgress)
            {
                return ApiResponse.Fail("Rides currently in progress cannot be cancelled.", 400, ResponseCodes.BadRequest);
            }

            var previousStatus = ride.Status;
            ride.Status = RideStatus.Cancelled;
            ride.CancellationReason = request.Reason.Trim();
            ride.CancelledAt = DateTime.UtcNow;

            rideRepository.Update(ride);

            await rideRepository.AddStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = previousStatus,
                NewStatus = RideStatus.Cancelled,
                ChangedBy = $"{role}:{userId}",
                Reason = request.Reason.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "RideCancellation",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });

            await rideRepository.SaveChangesAsync();
            await auditLogRepository.SaveChangesAsync();

            // Story 74: Notify relevant users when ride is cancelled
            if (ride.Passenger != null && !string.IsNullOrEmpty(ride.Passenger.Email))
            {
                await notificationService.SendEmailNotificationAsync(
                    ride.PassengerId,
                    ride.Passenger.Email,
                    "Ride Cancelled",
                    $"Your ride ({ride.Reference}) has been cancelled. Reason: {request.Reason}"
                );
            }

            if (ride.Driver != null && !string.IsNullOrEmpty(ride.Driver.Email))
            {
                await notificationService.SendEmailNotificationAsync(
                    ride.Driver.Id,
                    ride.Driver.Email,
                    "Assigned Ride Cancelled",
                    $"The ride ({ride.Reference}) you were assigned to has been cancelled."
                );
            }

            logger.LogInformation("Ride {RideId} cancelled by {Role} {UserId}", rideId, role, userId);

            var response = MapToResponse(ride, ride.Passenger, ride.Driver);
            return ApiResponse.Success("Ride cancelled successfully.", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel Ride {RideId} by User {UserId}", rideId, userId);
            return ApiResponse.Fail("An unexpected error occurred while cancelling the ride.", 500, ResponseCodes.ServerError);
        }
    }

    private static bool IsValidDriverTransition(RideStatus current, RideStatus target) =>
        (current, target) switch
        {
            (RideStatus.Accepted, RideStatus.DriverArriving) => true,
            (RideStatus.DriverArriving, RideStatus.DriverArrived) => true,
            (RideStatus.DriverArrived, RideStatus.InProgress) => true,
            (RideStatus.InProgress, RideStatus.Completed) => true,
            _ => false
        };

    private async Task<string> GenerateUniqueRideReferenceAsync()
    {
        while (true)
        {
            var randomHex = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var reference = $"RH-{DateTime.UtcNow:yyyyMMdd}-{randomHex}";
            if (!await rideRepository.ReferenceExistsAsync(reference))
            {
                return reference;
            }
        }
    }

    private static RideResponse MapToResponse(Ride ride, User? passenger, User? driver) =>
        new()
        {
            Id = ride.Id,
            Reference = ride.Reference ?? string.Empty,
            PassengerId = ride.PassengerId,
            PassengerName = passenger != null ? $"{passenger.FirstName} {passenger.LastName}".Trim() : string.Empty,
            DriverId = ride.DriverId,
            DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}".Trim() : null,
            PickupLocation = ride.PickupLocation ?? string.Empty,
            Destination = ride.Destination ?? string.Empty,
            Status = ride.Status,
            CancellationReason = ride.CancellationReason,
            CreatedAt = ride.CreatedAt,
            CompletedAt = ride.CompletedAt,
            CancelledAt = ride.CancelledAt
        };
}
