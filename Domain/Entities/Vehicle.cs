namespace Ride_Hailing_API.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public User? Driver { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Year { get; set; }
    public string? Color { get; set; }
    public int? Capacity { get; set; }
    public string? PlateNumber { get; set; }
}