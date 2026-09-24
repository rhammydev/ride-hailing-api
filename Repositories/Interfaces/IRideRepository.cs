using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(int id);
    Task<Ride?> GetByReferenceAsync(string reference);
    Task<bool> ReferenceExistsAsync(string reference);
    Task<List<Ride>> GetAvailableRidesAsync();
    Task<List<Ride>> GetPassengerRidesAsync(int passengerId);
    Task<List<Ride>> GetDriverRidesAsync(int driverId);
    Task<List<Ride>> GetAllRidesAsync();
    Task<Ride?> GetActiveRideForPassengerAsync(int passengerId);
    Task<Ride?> GetActiveRideForDriverAsync(int driverId);
    Task AddAsync(Ride ride);
    void Update(Ride ride);

    Task AddStatusHistoryAsync(RideStatusHistory history);
    Task<IReadOnlyList<RideStatusHistory>> GetStatusHistoryByRideIdAsync(int rideId);

    Task SaveChangesAsync();
}
