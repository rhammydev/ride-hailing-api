using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Repositories.Interfaces;

public interface IDriverRepository
{
    Task<Kyc?> GetKycByUserIdAsync(int userId);
    Task<Kyc?> GetKycByIdAsync(int id);
    Task<List<Kyc>> GetPendingDriversAsync();
    Task<List<Kyc>> GetAllDriversAsync();
    Task AddKycAsync(Kyc kyc);
    void UpdateKyc(Kyc kyc);

    Task<Vehicle?> GetVehicleByDriverIdAsync(int driverId);
    Task<bool> PlateNumberExistsAsync(string plateNumber);
    Task AddVehicleAsync(Vehicle vehicle);
    void UpdateVehicle(Vehicle vehicle);

    Task SaveChangesAsync();
}
