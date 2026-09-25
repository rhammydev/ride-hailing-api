using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class RideRepository(AppDbContext context) : IRideRepository
{
    public Task<Ride?> GetByIdAsync(int id) =>
        context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<Ride?> GetByReferenceAsync(string reference) =>
        context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .FirstOrDefaultAsync(r => r.Reference == reference);

    public Task<bool> ReferenceExistsAsync(string reference) =>
        context.Rides.AnyAsync(r => r.Reference == reference);

    public async Task<List<Ride>> GetAvailableRidesAsync() =>
        await context.Rides
            .Include(r => r.Passenger)
            .Where(r => r.Status == RideStatus.Requested && r.DriverId == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Ride>> GetPassengerRidesAsync(int passengerId) =>
        await context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .Where(r => r.PassengerId == passengerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Ride>> GetDriverRidesAsync(int driverId) =>
        await context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .Where(r => r.DriverId == driverId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Ride>> GetAllRidesAsync() =>
        await context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<Ride?> GetActiveRideForPassengerAsync(int passengerId) =>
        context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .Where(r => r.PassengerId == passengerId &&
                        r.Status != RideStatus.Completed &&
                        r.Status != RideStatus.Cancelled)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

    public Task<Ride?> GetActiveRideForDriverAsync(int driverId) =>
        context.Rides
            .Include(r => r.Passenger)
            .Where(r => r.DriverId == driverId &&
                        r.Status != RideStatus.Completed &&
                        r.Status != RideStatus.Cancelled)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(Ride ride) =>
        await context.Rides.AddAsync(ride);

    public void Update(Ride ride) =>
        context.Rides.Update(ride);

    public async Task AddStatusHistoryAsync(RideStatusHistory history) =>
        await context.RidesStatusHistories.AddAsync(history);

    public async Task<List<RideStatusHistory>> GetStatusHistoryByRideIdAsync(int rideId) =>
        await context.RidesStatusHistories
            .Where(h => h.RideId == rideId)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
