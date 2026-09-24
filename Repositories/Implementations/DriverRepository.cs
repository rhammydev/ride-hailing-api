using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Data;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.Repositories.Interfaces;

namespace Ride_Hailing_API.Repositories.Implementations;

public class DriverRepository(AppDbContext context) : IDriverRepository
{
    public Task<Kyc?> GetKycByUserIdAsync(int userId) =>
        context.Kycs
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

    public Task<Kyc?> GetKycByIdAsync(int id) =>
        context.Kycs
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Kyc>> GetPendingDriversAsync() =>
        await context.Kycs
            .Include(x => x.User)
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

    public async Task<List<Kyc>> GetAllDriversAsync() =>
        await context.Kycs
            .Include(x => x.User)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

    public async Task AddKycAsync(Kyc kyc) =>
        await context.Kycs.AddAsync(kyc);

    public void UpdateKyc(Kyc kyc) =>
        context.Kycs.Update(kyc);

    public Task<Vehicle?> GetVehicleByDriverIdAsync(int driverId) =>
        context.Vehicles.FirstOrDefaultAsync(x => x.DriverId == driverId);

    public Task<bool> PlateNumberExistsAsync(string plateNumber) =>
        context.Vehicles.AnyAsync(x => x.PlateNumber == plateNumber);

    public async Task AddVehicleAsync(Vehicle vehicle) =>
        await context.Vehicles.AddAsync(vehicle);

    public void UpdateVehicle(Vehicle vehicle) =>
        context.Vehicles.Update(vehicle);

    public Task SaveChangesAsync() =>
        context.SaveChangesAsync();
}
